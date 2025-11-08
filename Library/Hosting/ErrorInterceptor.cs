using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace SoEx.Hosting
{
    public class ErrorInterceptor : AsyncInterceptorBase, IInterceptor
    {
        private readonly ILogger<ErrorInterceptor> _logger;

        public ErrorInterceptor(ILogger<ErrorInterceptor> logger)
        {
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            this.ToInterceptor().Intercept(invocation);
        }

        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            try
            {
                await proceed(invocation, proceedInfo).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ExceptionMessage}", ex.Message);
                throw;
            }
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            try
            {
                TResult? result = await proceed(invocation, proceedInfo).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ExceptionMessage}", ex.Message);
                throw;
            }
        }
    }
}
