using System.Diagnostics;
using Autofac;

namespace SoEx
{
    public static class Container
    {
        private static readonly AsyncLocal<ILifetimeScope> s_asyncLocalLifetimeScope = new();

        public static TService Resolve<TService>() where TService : notnull
        {
            Debug.Assert(s_asyncLocalLifetimeScope.Value is not null);
            return s_asyncLocalLifetimeScope.Value.Resolve<TService>();
        }

        public static ILifetimeScope BeginLifetimeScopeAsyncLocal(this ILifetimeScope parent)
        {
            ILifetimeScope childScope = parent.BeginLifetimeScope();
            s_asyncLocalLifetimeScope.Value = childScope;
            return childScope;
        }

        public static void SetScopeAsyncLocal(this ILifetimeScope parent)
        {
            s_asyncLocalLifetimeScope.Value = parent;
        }
    }
}
