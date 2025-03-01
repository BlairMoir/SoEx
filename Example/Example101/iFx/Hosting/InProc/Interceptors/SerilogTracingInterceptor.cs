using Castle.DynamicProxy;
using SerilogTracing;

namespace Example101.iFx
{
    public class SerilogTracingInterceptor : AsyncInterceptorBase, IInterceptor
    {
        private readonly Serilog.ILogger _logger;
        public SerilogTracingInterceptor(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            this.ToInterceptor().Intercept(invocation);
        }

        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            using (LoggerActivity activity = _logger.StartActivity("{Service} {Operation}", invocation.TargetType?.Name, invocation.Method?.Name))
            {
                await proceed(invocation, proceedInfo).ConfigureAwait(false);
            }
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            using (LoggerActivity activity = _logger.StartActivity("{Service} {Operation}", invocation.TargetType?.Name, invocation.Method?.Name))
            {
                TResult? result = await proceed(invocation, proceedInfo).ConfigureAwait(false);
                return result;
            }
        }
    }
}
