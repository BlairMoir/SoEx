using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;


namespace SoEx.DynamicProxy
{
    public class InvocationInterceptor : AsyncInterceptorBase, IInterceptor
    {
        private readonly ILogger<InvocationInterceptor> _logger;

        public InvocationInterceptor(ILogger<InvocationInterceptor> logger)
        {
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            this.ToInterceptor().Intercept(invocation);
        }

        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            _logger.LogInformation("{Service} {Operation} Started", invocation.TargetType?.Name, invocation.Method.Name);
            await proceed(invocation, proceedInfo).ConfigureAwait(false);
            _logger.LogInformation("{Service} {Operation} Ended", invocation.TargetType?.Name, invocation.Method.Name);
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            _logger.LogInformation("{Service} {Operation} Started", invocation.TargetType?.Name, invocation.Method.Name);
            TResult? result = await proceed(invocation, proceedInfo).ConfigureAwait(false);
            _logger.LogInformation("{Service} {Operation} Ended", invocation.TargetType?.Name, invocation.Method.Name);
            return result;
        }
    }
}
