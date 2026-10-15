using System.Diagnostics;
using Autofac;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;

namespace SoEx.Hosting
{
    public class SubSystemScope : ISubSystemScope
    {
        private ILifetimeScope? lifetimeScope;

        public ISubSystemHost For<I>()
        {
            Debug.Assert(lifetimeScope is not null);
            return (ISubSystemHost)lifetimeScope.Resolve(typeof(I));
        }

        public void StoreLifetimeScope(ILifetimeScope scope)
        {
            if (lifetimeScope is null)
            {
                lifetimeScope = scope;
            }
        }
    }
}
