using System.Diagnostics;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapr.Client;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using ProtoBuf.Grpc.Client;
using ProtoBuf.Grpc.Server;
using SoEx.Context;
using SoEx.Grpc;


namespace SoEx.Dapr
{
    public static class Host
    {
        private static readonly List<Type> s_standardInterceptors = [typeof(ScopeInterceptor), typeof(Grpc.InvocationInterceptor), typeof(ErrorInterceptor)];

        public static IHostApplicationBuilder DaprIfx(this WebApplicationBuilder hostBuilder, Func<Type,string> appIdConvention, Type[] clientInterfaces)
        {
            return DaprIfx(hostBuilder, appIdConvention, clientInterfaces, [.. s_standardInterceptors]);
        }
        public static IHostApplicationBuilder DaprIfx(this WebApplicationBuilder hostBuilder, Func<Type,string> appIdConvention, Type[] clientInterfaces, Func<List<Type>, Type[]> interceptorFactory)
        {

            return DaprIfx(hostBuilder, appIdConvention, clientInterfaces, interceptorFactory.Invoke(s_standardInterceptors));
        }

        private static IHostApplicationBuilder DaprIfx(this WebApplicationBuilder hostBuilder, Func<Type,string> appIdConvention, Type[] clientInterfaces, Type[] interceptors)
        {
            var factoryProvider = new AutofacServiceProviderFactory(builder =>
            {
                builder.RegisterType<ScopeInterceptor>();
                builder.RegisterType<AmbientContext>().AsImplementedInterfaces().InstancePerLifetimeScope();
                builder.RegisterBuildCallback(scope => ContainerFactory.ForRoot(() => scope));
                RegisterGrpcClients(builder, appIdConvention, clientInterfaces);
            });
            hostBuilder.Host.UseServiceProviderFactory(factoryProvider);
            hostBuilder.Services.AddCodeFirstGrpc(
                options =>
                {
                    foreach (Type interceptor in interceptors)
                    {
                        options.Interceptors.Add(interceptor);
                    }
                });
            hostBuilder.Services.AddCodeFirstGrpcReflection();
            return hostBuilder;
        }

        public static WebApplication DaprService(this WebApplication app, Type[] serviceTypes)
        {
            foreach (Type service in serviceTypes)
            {
                MapGrpcService(app, service);
            }
            app.MapGrpcService<AppCallBackService>();
            app.MapCodeFirstGrpcReflectionService();
            return app;
        }

        private static void MapGrpcService(WebApplication app, Type? service)
        {
            MethodInfo? methodInfo = typeof(GrpcEndpointRouteBuilderExtensions).GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService));
            Debug.Assert(methodInfo is not null);
            Debug.Assert(service is not null);
            MethodInfo method = methodInfo.MakeGenericMethod(service);
            method.Invoke(null, [app]);
        }

        private static void RegisterGrpcClients(ContainerBuilder builder,Func<Type,string> appIdConvention, Type[] serviceInterfaces)
        {
            foreach (Type serviceInterfaceType in serviceInterfaces)
            {
                Debug.Assert(serviceInterfaceType.Namespace is not null);
                string appId = appIdConvention.Invoke(serviceInterfaceType);                
                builder.Register((c) => ServiceClientFactory(c, serviceInterfaceType, appId))
                .As(serviceInterfaceType);
            }
        }

        private static object ServiceClientFactory(IComponentContext c, Type serviceInterfaceType, string appId)
        {
            CallInvoker invoker = DaprClient.CreateInvocationInvoker(appId: appId)
            .Intercept(c.Resolve<ScopeInterceptor>());

            MethodInfo? methodInfo = typeof(GrpcClientFactory).GetMethods(
            ).Where(m =>
                m.Name == nameof(GrpcClientFactory.CreateGrpcService)
                && m.ContainsGenericParameters
                && m.GetParameters().Any(a => a.ParameterType == typeof(CallInvoker))
                ).First();
            Debug.Assert(methodInfo is not null);
            MethodInfo method = methodInfo.MakeGenericMethod(serviceInterfaceType);
            object? client = method.Invoke(null, [invoker, null]);
            Debug.Assert(client is not null);
            return client;
        }
    }
}