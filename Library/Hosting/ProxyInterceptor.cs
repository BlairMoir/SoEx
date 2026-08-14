using System.Diagnostics;
using Castle.DynamicProxy;
using SoEx.Abstractions;
using SoEx.Channel;
using SoEx.Context;
using SoEx.Topology;

namespace SoEx.Hosting
{
    public class ProxyInterceptor : AsyncInterceptorBase, IInterceptor
    {
        TransportFactory _transportFactory;
        AmbientContext _ambientContext;
        FrameworkContext _frameworkContext;
        IChannelPipeline _channelPipline;

        public ProxyInterceptor(TransportFactory transportFactory, IAmbientContext ambientContext, IFrameworkContext frameworkContext , IChannelPipeline channelPipline)
        {
            _transportFactory = transportFactory;
            _ambientContext = (AmbientContext)ambientContext;
            _frameworkContext = (FrameworkContext)frameworkContext;
            _channelPipline = channelPipline;
        }

        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;
            if (
                returnType != typeof(Task) &&
                !(returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)))
            {
                throw new NotSupportedException("SoEx contracts must be async");
            }

            this.ToInterceptor().Intercept(invocation);
        }

        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(ProxyInterceptor)} {invocation.Method.DeclaringType} {invocation.Method.Name}"))
            {
                try
                {
                    Type targetInterface = invocation.Method.DeclaringType ?? throw new ArgumentException("Proxy Interceptor requires an interface");
                    IChannel channel = _transportFactory.Client(targetInterface) as IChannel ?? throw new NullReferenceException();
                    var invocationRequest = new InvocationRequest() { ActivityId = activity?.Id, MethodName = invocation.Method.Name, Arguments = invocation.Arguments, AmbientContext = _ambientContext.Serialize(), FrameworkContext =  _frameworkContext.Serialize() };
                    var invocationResponse = await _channelPipline.ClientPipeLine(invocationRequest, channel, activity).ConfigureAwait(false);
                    _ambientContext.Deserialize(invocationResponse.AmbientContext);
                }
                catch
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }
        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(ProxyInterceptor)} {invocation.Method.DeclaringType} {invocation.Method.Name}"))
            {
                try
                {
                    Type targetInterface = invocation.Method.DeclaringType ?? throw new ArgumentException("Proxy Interceptor requires an interface");
                    IChannel channel = _transportFactory.Client(targetInterface) as IChannel ?? throw new NullReferenceException();
                    var invocationRequest = new InvocationRequest() { ActivityId = activity?.Id, MethodName = invocation.Method.Name, Arguments = invocation.Arguments, HasResult = true, AmbientContext = _ambientContext.Serialize(), FrameworkContext =  _frameworkContext.Serialize() };
                    var invocationResponse = await _channelPipline.ClientPipeLine(invocationRequest, channel, activity).ConfigureAwait(false);
                    _ambientContext.Deserialize(invocationResponse.AmbientContext);
                    return invocationResponse.Response.ToResponseType<TResult>()!;
                }
                catch
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }
    }
}
