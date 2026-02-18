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
        AmbientContext _context;
        IChannelPipeline _channelPipline;
        public ProxyInterceptor(TransportFactory transportFactory, IAmbientContext context, IChannelPipeline channelPipline)
        {
            _transportFactory = transportFactory;
            _context = (AmbientContext)context;
            _channelPipline = channelPipline;
        }

        public void Intercept(IInvocation invocation)
        {
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
                    var invocationRequest = new InvocationRequest() { ActivityId = activity?.Id, MethodName = invocation.Method.Name, Arguments = invocation.Arguments, AmbientContext = _context.Serialize() };
                    var invocationResponse = await _channelPipline.ClientPipeLine(invocationRequest, channel, activity).ConfigureAwait(false);
                    _context.Deserialize(invocationResponse.AmbientContext);
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
                    var invocationRequest = new InvocationRequest() { ActivityId = activity?.Id, MethodName = invocation.Method.Name, Arguments = invocation.Arguments, TResult = typeof(TResult), AmbientContext = _context.Serialize() };
                    var invocationResponse = await _channelPipline.ClientPipeLine(invocationRequest, channel, activity).ConfigureAwait(false);
                    _context.Deserialize(invocationResponse.AmbientContext);
                    return (TResult)invocationResponse.Response!;
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
