using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace SoEx.PubSub
{
    public interface IPublishInterceptor<I> : IInterceptor where I : class;
}