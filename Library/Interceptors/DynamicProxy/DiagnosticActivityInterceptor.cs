using System.Diagnostics;
using Castle.DynamicProxy;

namespace SoEx.DynamicProxy
{
    public class DiagnosticActivityInterceptor : AsyncInterceptorBase, IInterceptor
    {
        private static readonly ActivitySource s_source = new("SoEx.InProc", "1.0.0");


        public void Intercept(IInvocation invocation)
        {
            this.ToInterceptor().Intercept(invocation);
        }

        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            using (Activity? activity = s_source.StartActivity($"{invocation.TargetType?.Namespace}.{invocation.TargetType?.Name}.{invocation.Method?.Name}"))
            {
                await proceed(invocation, proceedInfo).ConfigureAwait(false);
            }
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            using (Activity? activity = s_source.StartActivity($"{invocation.TargetType?.Namespace}.{invocation.TargetType?.Name}.{invocation.Method?.Name}"))
            {
                TResult? result = await proceed(invocation, proceedInfo).ConfigureAwait(false);
                return result;
            }
        }
    }
}
