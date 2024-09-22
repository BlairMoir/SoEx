using Autofac;
using Grpc.Context;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using SoEx.Context;
using SoEx.Grpc.Context;

namespace SoEx.Grpc
{
    public class ScopeInterceptor : Interceptor
    {
        ILogger<ScopeInterceptor> _logger;
        IEnumerable<IContextFlowPolicy> _policies;
        public ScopeInterceptor(ILogger<ScopeInterceptor> logger, IEnumerable<IContextFlowPolicy> policies)
        {
            _logger = logger;
            _policies = policies;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            IAmbientContext callerContext = Container.Resolve<IAmbientContext>();
            var invokedRequestContext = new GrpcClientRequestContext();
            Copy(callerContext, invokedRequestContext);
            invokedRequestContext.UpdateOutgoingGrpcClientContext(ref context);
            AsyncUnaryCall<TResponse> call = continuation(request, context);
            var invokedResponseContext = new GrpcClientResponseContext(call.ResponseHeadersAsync.Result);
            Copy(invokedResponseContext, callerContext);
            return call;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var callerRequestContext = new GrpcServiceRequestContext(context.RequestHeaders ?? new Metadata());
            using (BeginLifetimeScopeWithServiceContext(context))
            {
                IAmbientContext invokedContext = InvokedContext(callerRequestContext);
                using (_logger.BeginScope(ScopeProperties(invokedContext)))
                {
                    TResponse result = await continuation(request, context);
                    var callerResponseContext = new GrpcServiceResponseContext();
                    FlowContextToCaller(callerResponseContext, invokedContext);
                    callerResponseContext.UpdateResponseGrpcServiceContext(context);
                    return result;
                }
            }
        }

        public static ILifetimeScope BeginLifetimeScopeWithServiceContext(ServerCallContext context)
        {
            string[] parts = context.Method.Split("/");
            string fullName = parts[parts.Length - 2];
            GrpcServiceContext serviceContext = new(fullName);
            return Container.BeginLocalLifetimeScope(builder => builder.RegisterInstance(serviceContext).As<ServiceContext>());
        }

        private IAmbientContext InvokedContext(IAmbientContext callerContext)
        {
            IAmbientContext invokedContext = Container.Resolve<IAmbientContext>();
            FlowIncoming(callerContext, invokedContext);
            return invokedContext;
        }

        private void FlowIncoming(IAmbientContext caller, IAmbientContext invoked)
        {
            foreach (IContextFlowPolicy policy in _policies)
            {
                policy.Incoming(caller, invoked);
            }
        }

        private void FlowContextToCaller(IAmbientContext caller, IAmbientContext invoked)
        {
            foreach (IContextFlowPolicy policy in _policies)
            {
                policy.Outgoing(invoked, caller);
            }
        }

        private void Copy(IAmbientContext source, IAmbientContext destination)
        {
            foreach (IContextFlowPolicy policy in _policies)
            {
                policy.Copy(source, destination);
            }
        }

        private Dictionary<string, object> ScopeProperties(IAmbientContext invokedContext)
        {
            return _policies.SelectMany(s => s.ScopeProperties(invokedContext)).ToDictionary();
        }
    }
}
