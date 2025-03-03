using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using SoEx.PubSub;

namespace Test.Unit.Membership
{
    public class PublishInterceptor <I> : AsyncInterceptorBase, IPublishInterceptor<I> where I : class
    {
        IPublishedMessageAssertions _assertions;
        public PublishInterceptor(IPublishedMessageAssertions assertions)
        {
            _assertions = assertions;
        }

        public void Intercept(Castle.DynamicProxy.IInvocation invocation){
            this.ToInterceptor().Intercept(invocation);
        }

        protected override Task InterceptAsync(Castle.DynamicProxy.IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<Castle.DynamicProxy.IInvocation, IInvocationProceedInfo, Task> proceed){
            _assertions.OnPublished(invocation.Method.Name, invocation.Arguments);
            return Task.CompletedTask;
        }

        protected override Task<TResult> InterceptAsync<TResult>(Castle.DynamicProxy.IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<Castle.DynamicProxy.IInvocation, IInvocationProceedInfo, Task<TResult>> proceed) => throw new NotImplementedException();
    }
}