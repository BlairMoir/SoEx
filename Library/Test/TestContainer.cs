using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using SoEx.Context;

namespace SoEx.Test
{
    public static class TestContainer
    {
        private static readonly ProxyGenerator s_proxyGenerator = new ProxyGenerator();

        static TestContainer()
        {
            ContainerFactory.ForRoot(TestRootScopeFactory);
        }

        private static ILifetimeScope TestRootScopeFactory()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<LoggerFactory>()
                .As<ILoggerFactory>()
                .SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();
            builder.RegisterType<AmbientContext>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<DynamicProxy.ScopeInterceptor>();
            return builder.Build();
        }

        internal static IDisposable CreateTestScope(Type[] types, Type[] policies)
        {
            return Container.BeginLocalLifetimeScope(configuration =>
            {
                configuration.RegisterTypes(types).As(t => t.GetInterfaces())
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(DynamicProxy.ScopeInterceptor));
                configuration.RegisterTypes(policies).AsImplementedInterfaces();
            });
        }

        internal static IDisposable CreateTestScope(object[] mocks)
        {
            return Container.BeginLocalLifetimeScope(configuration =>
            {
                foreach (object mock in mocks)
                {
                    DynamicProxy.ScopeInterceptor scopeInterceptor = Container.Resolve<DynamicProxy.ScopeInterceptor>();
                    Type interfaceType = mock.GetType().GetInterfaces().First();
                    object mockProxy = s_proxyGenerator.CreateInterfaceProxyWithTargetInterface(interfaceType, mock, scopeInterceptor.ToInterceptor());
                    configuration.RegisterInstance(mockProxy).As(interfaceType);
                }
            });
        }
    }
}
