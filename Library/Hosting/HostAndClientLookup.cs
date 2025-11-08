using System.Diagnostics;
using Autofac;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;

namespace SoEx.Hosting
{
    public class HostAndClientLookup : IHostAndClientLookup
    {
        public HostAndClientLookup()
        {
        }

        private ILifetimeScope? lifetimeScope;
        public ISubSystemHost For(Type type)
        {
            Debug.Assert(lifetimeScope is not null);
            return (ISubSystemHost)lifetimeScope.Resolve(type);
        }

        public ISubSystemHost For<I>()
        {
            return For(typeof(I));
        }

        public ISubSystemHost[] SubSystemHosts => lifetimeScope?.Resolve<ISubSystemHost[]>() ?? [];

        public void StoreLifetimeScope(ILifetimeScope scope)
        {
            if (lifetimeScope is null)
            {
                lifetimeScope = scope;
            }
        }
    }
}
