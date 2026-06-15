using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;

namespace SoEx.Hosting
{
    public class ErrorInterceptor : AsyncInterceptorBase, IInterceptor
    {
        private readonly ILogger<ErrorInterceptor> _logger;
        private readonly ITelemetryConfidentiality _telemetryConfidentiality;

        public ErrorInterceptor(ILogger<ErrorInterceptor> logger, ITelemetryConfidentiality  telemetryConfidentiality)
        {
            _logger = logger;
            _telemetryConfidentiality =  telemetryConfidentiality;
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
                _logger.LogError("{ExceptionType}, {ExceptionStackTrace}", ex.GetType(), ex.StackTrace);
                _logger.LogDebug("{ExceptionMessage}", _telemetryConfidentiality.Protect(ex.Message));
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
                _logger.LogError("{ExceptionType}, {ExceptionStackTrace}", ex.GetType(), ex.StackTrace);
                _logger.LogDebug("{ExceptionMessage}", _telemetryConfidentiality.Protect(ex.Message));
                throw;
            }
        }
    }
}
