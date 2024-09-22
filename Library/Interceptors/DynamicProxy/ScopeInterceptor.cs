using System.Diagnostics;
using Autofac;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using SoEx.Context;
using SoEx.DynamicProxy.Context;

namespace SoEx.DynamicProxy
{
    public class ScopeInterceptor : AsyncInterceptorBase, IInterceptor
    {
        private readonly ILogger<ScopeInterceptor> _logger;
        private readonly IEnumerable<IContextFlowPolicy> _policies;

        public ScopeInterceptor(ILogger<ScopeInterceptor> logger, IEnumerable<IContextFlowPolicy> policies)
        {
            _logger = logger;
            _policies = policies;
        }

        public void Intercept(IInvocation invocation)
        {
            this.ToInterceptor().Intercept(invocation);
        }

        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            IAmbientContext callerContext = CallerContext();
            bool calledFromService = Container.Resolve<IEnumerable<ServiceContext>>().Any();
            using (BeginLifetimeScopeWithServiceContext(invocation))
            {
                IAmbientContext invokedContext = InvokedContext(callerContext);
                using (_logger.BeginScope(ScopeProperties(invokedContext)))
                {
                    await proceed(invocation, proceedInfo).ConfigureAwait(false);
                    FlowContextToCaller(callerContext, invokedContext, calledFromService);
                }
            }
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            IAmbientContext callerContext = CallerContext();
            bool calledFromService = Container.Resolve<IEnumerable<ServiceContext>>().Any();
            using (BeginLifetimeScopeWithServiceContext(invocation))
            {
                IAmbientContext invokedContext = InvokedContext(callerContext);
                using (_logger.BeginScope(ScopeProperties(invokedContext)))
                {
                    var result = await proceed(invocation, proceedInfo).ConfigureAwait(false);
                    FlowContextToCaller(callerContext, invokedContext, calledFromService);
                    return result;
                };
            }
        }

        public static ILifetimeScope BeginLifetimeScopeWithServiceContext(IInvocation invocation)
        {
            Debug.Assert(invocation.TargetType.FullName is not null);
            DynamicProxyServiceContext serviceContext = new(invocation.TargetType.FullName);
            return Container.BeginLocalLifetimeScope(builder => builder.RegisterInstance(serviceContext).As<ServiceContext>());
        }

        private void FlowContextToCaller(IAmbientContext caller, IAmbientContext invoked, bool calledFromService)
        {
            if (!calledFromService)
                return;
            foreach (IContextFlowPolicy policy in _policies)
            {
                policy.Outgoing(invoked, caller);
            }
        }

        private void FlowIncoming(IAmbientContext caller, IAmbientContext invoked)
        {
            foreach (IContextFlowPolicy policy in _policies)
            {
                policy.Incoming(caller, invoked);
            }
        }

        private static IAmbientContext CallerContext()
        {
            IAmbientContext callerContext = Container.Resolve<IAmbientContext>();
            return callerContext;
        }

        private IAmbientContext InvokedContext(IAmbientContext callerContext)
        {
            IAmbientContext invokedContext = Container.Resolve<IAmbientContext>();
            FlowIncoming(callerContext, invokedContext);
            return invokedContext;
        }

        private Dictionary<string, object> ScopeProperties(IAmbientContext invokedContext)
        {
            return _policies.SelectMany(s => s.ScopeProperties(invokedContext)).ToDictionary();
        }
    }
}
