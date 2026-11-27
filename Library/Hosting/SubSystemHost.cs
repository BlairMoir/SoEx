using Autofac;
using SoEx.Abstractions;

namespace SoEx.Hosting
{
    public class SubSystemHost<T, I> : ISubSystemHost<I>, ISubSystemHost
    {
        ILifetimeScope _hostLifetimeScope;
        public SubSystemHost(ILifetimeScope scope)
        {
            _hostLifetimeScope = scope;
        }

        public ILifetimeScope BeginRequestLifetimeScope()
        {
            return _hostLifetimeScope.BeginLifetimeScopeAsyncLocal();
        }
    }
}
