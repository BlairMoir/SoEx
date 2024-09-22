using Autofac;

namespace SoEx
{
    public static class Container
    {
        private static readonly ILifetimeScope s_rootScope;
        private static readonly AsyncLocal<ILifetimeScope> s_asyncLocalLifetimeScope = new();

        static Container()
        {
            s_rootScope = ContainerFactory.Create();
        }

        private static ILifetimeScope ActiveLifetimeScope()
        {
            return s_asyncLocalLifetimeScope.Value ?? s_rootScope;
        }

        public static TService Resolve<TService>() where TService : notnull
        {
            return ActiveLifetimeScope().Resolve<TService>();
        }

        public static ILifetimeScope BeginLocalLifetimeScope(Action<ContainerBuilder> builder)
        {
            ILifetimeScope localLifetimeScope = ActiveLifetimeScope().BeginLifetimeScope(builder);
            s_asyncLocalLifetimeScope.Value = localLifetimeScope;
            return localLifetimeScope;
        }

        public static ILifetimeScope BeginLocalLifetimeScope()
        {
            ILifetimeScope localLifetimeScope = ActiveLifetimeScope().BeginLifetimeScope();
            s_asyncLocalLifetimeScope.Value = localLifetimeScope;
            return localLifetimeScope;
        }
    }

    public static class ContainerFactory
    {
        static Func<ILifetimeScope> s_scopeFactory = new(() => new ContainerBuilder().Build());

        public static void ForRoot(Func<ILifetimeScope> rootScopeFactory)
        {
            s_scopeFactory = rootScopeFactory;
        }

        internal static ILifetimeScope Create()
        {
            return s_scopeFactory.Invoke();
        }
    }
}
