using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
using Microsoft.Extensions.Hosting;
using SoEx.Context;
using SoEx.DynamicProxy;

namespace SoEx.InProc
{
    public static class HostExtensions
    {
        private static readonly List<Type> s_standardInterceptors = [typeof(DiagnosticActivityInterceptor), typeof(ScopeInterceptor), typeof(InvocationInterceptor), typeof(ErrorInterceptor)];

        public static IHostBuilder InProcIfx(this IHostBuilder hostBuilder, params Type[] serviceTypes)
        {
            return InProcIfx(hostBuilder, [.. s_standardInterceptors], serviceTypes);
        }

        public static IHostBuilder InProcIfx(this IHostBuilder hostBuilder, Func<List<Type>, Type[]> interceptorFactory, params Type[] serviceTypes)
        {
            return InProcIfx(hostBuilder, [.. interceptorFactory.Invoke(s_standardInterceptors)], serviceTypes);
        }

        private static IHostBuilder InProcIfx(this IHostBuilder hostBuilder, Type[] interceptors, params Type[] serviceTypes)
        {
            var factoryProvider = new AutofacServiceProviderFactory(builder =>
            {
                builder.RegisterTypes(interceptors);
                builder.RegisterType<AmbientContext>().AsImplementedInterfaces().InstancePerLifetimeScope();
                builder.RegisterBuildCallback(scope => ContainerFactory.ForRoot(() => scope));
                RegisterServiceTypes(builder, interceptors, serviceTypes);
            });
            return hostBuilder.UseServiceProviderFactory(factoryProvider);
        }

        private static void RegisterServiceTypes(ContainerBuilder builder, Type[] interceptors, Type[] types)
        {
            builder.RegisterTypes(types)
                    .As(t => t.GetInterfaces())
                    .EnableInterfaceInterceptors()
                    .InterceptedBy(interceptors);
        }
    }
}
