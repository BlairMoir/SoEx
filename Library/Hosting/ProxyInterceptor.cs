using System.Diagnostics;
using Castle.DynamicProxy;
using SoEx.Abstractions;
using SoEx.Channel;
using SoEx.Context;
using SoEx.Hosting.Extensions;
using SoEx.Topology;

namespace SoEx.Hosting
{
    public class ProxyInterceptor : AsyncInterceptorBase, IInterceptor
    {
        TransportFactory _transportFactory;
        AmbientContext _ambientContext;
        FrameworkContext _frameworkContext;
        IChannelPipeline _channelPipline;
        Type _contract;

        public ProxyInterceptor(Type contract, TransportFactory transportFactory, IAmbientContext ambientContext, IFrameworkContext frameworkContext, IChannelPipeline channelPipline)
        {
            _contract = contract;
            _transportFactory = transportFactory;
            _ambientContext = (AmbientContext)ambientContext;
            _frameworkContext = (FrameworkContext)frameworkContext;
            _channelPipline = channelPipline;
        }

        public void Intercept(IInvocation invocation)
        {
            Type returnType = invocation.Method.ReturnType;
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
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(ProxyInterceptor)} {_contract} {invocation.Method.Name}"))
            {
                try
                {
                    IChannel channel = _transportFactory.Client(_contract) as IChannel ?? throw new NullReferenceException();
                    var invocationRequest = new InvocationRequest() { ActivityId = activity?.Id, MethodName = invocation.Method.Name, Arguments = invocation.Arguments, AmbientContext = _ambientContext.Serialize(), FrameworkContext = _frameworkContext.Serialize() };
                    InvocationResponse invocationResponse = await _channelPipline.ClientPipeLine(invocationRequest, channel, activity).ConfigureAwait(false);
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
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(ProxyInterceptor)} {_contract} {invocation.Method.Name}"))
            {
                try
                {
                    IChannel channel = _transportFactory.Client(_contract) as IChannel ?? throw new NullReferenceException();
                    var invocationRequest = new InvocationRequest() { ActivityId = activity?.Id, MethodName = invocation.Method.Name, Arguments = invocation.Arguments, HasResult = true, AmbientContext = _ambientContext.Serialize(), FrameworkContext = _frameworkContext.Serialize() };
                    InvocationResponse invocationResponse = await _channelPipline.ClientPipeLine(invocationRequest, channel, activity).ConfigureAwait(false);
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
