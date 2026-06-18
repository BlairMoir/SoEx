using System.Diagnostics;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Channel;
using SoEx.Context;
using SoEx.Endpoint;
using SoEx.Hosting.Default;
using SoEx.Topology;

namespace SoEx.Hosting
{
    public static class HostExtensions
    {
        private static readonly ProxyGenerator s_proxyGenerator = new ProxyGenerator();
        public static IHostApplicationBuilder SoEx(this IHostApplicationBuilder hostApplicationBuilder, Topology.System systemTopology)
        {
            hostApplicationBuilder.ConfigureContainer(
                new AutofacServiceProviderFactory(hostApplicationContainer =>
                {
                    hostApplicationContainer.RegisterSoEx(systemTopology);
                }
            ));
            return hostApplicationBuilder;
        }

        public static IHostApplicationBuilder SoEx(this IHostApplicationBuilder hostApplicationBuilder, Topology.Host hostTopology, IPipeline? pipeline = null)
        {
            hostApplicationBuilder.ConfigureContainer(
                new AutofacServiceProviderFactory(hostApplicationContainer =>
                {
                    hostApplicationContainer.RegisterSoEx(hostTopology, pipeline);
                }
            ));
            return hostApplicationBuilder;
        }

        public static ContainerBuilder RegisterSoEx(this ContainerBuilder hostApplicationContainer, Topology.System systemTopology)
        {
            hostApplicationContainer.RegisterSystem(systemTopology);
            return hostApplicationContainer;
        }

        public static ContainerBuilder RegisterSoEx(this ContainerBuilder hostApplicationContainer, Topology.Host hostTopology, IPipeline? pipeline)
        {
            var componentPipeline = pipeline ?? new DefaultPipeline();
            hostApplicationContainer.RegisterComponent(hostTopology, componentPipeline);
            return hostApplicationContainer;
        }

        private static void RegisterPipeline(this ContainerBuilder container, IPipeline pipeline)
        {
            container.RegisterType<EndpointLifetimeService>().As<IHostedService>();
            container.RegisterTypes(pipeline.ServiceInterceptors);
            container.RegisterType<EndpointPipeline>().As<IEndpointPipeline>();
            container.RegisterType<ChannelPipeline>().As<IChannelPipeline>();
            container.RegisterType<RegisteredEndpoints>().SingleInstance().AsSelf();
            container.RegisterType<ProxyFactory>().AsSelf();
            container.RegisterType<ProxyInterceptor>().AsSelf();
            container.RegisterType<TransportFactory>().AsSelf();
            container.RegisterType(pipeline.MessageSerializer).As<IMessageSerializer>();
            container.RegisterType(pipeline.Dispatcher).As<IDispatcher>();
            container.RegisterType(pipeline.TelemetryConfidentiality).As<ITelemetryConfidentiality>();
            container.RegisterType(pipeline.MessageProtection).As<IMessageProtection>();
            container.RegisterType<AmbientContext>().As<IAmbientContext>().InstancePerLifetimeScope();
            container.RegisterType<FrameworkContext>().As<IFrameworkContext>().InstancePerLifetimeScope();
        }

        private static void RegisterSystem(this ContainerBuilder hostApplicationContainer, Topology.System systemTopology)
        {
            var pipeline = systemTopology.Defaults ?? new DefaultPipeline();
            hostApplicationContainer.RegisterPipeline(pipeline);
            hostApplicationContainer.RegisterProxies(systemTopology.Clients);
            hostApplicationContainer.RegisterSubSystems(systemTopology.SubSystems, pipeline);
        }

        private static void RegisterComponent(this ContainerBuilder hostApplicationContainer, Topology.Host hostTopology, IPipeline pipeline)
        {
            hostApplicationContainer.RegisterPipeline(pipeline);
            RegisterComponentHost(hostApplicationContainer, hostTopology, pipeline);
        }

        private static void CreateIsolatedScope(this ContainerBuilder parentContainer, Action<ILifetimeScope, ContainerBuilder> callback)
        {
            HostAndClientLookup hostAndClientLookupProcess = new HostAndClientLookup();
            parentContainer.RegisterInstance(hostAndClientLookupProcess).As<Abstractions.IHostAndClientLookup>();
            parentContainer.RegisterBuildCallback(parentScope =>
            {
                ILifetimeScope hostScope = parentScope.BeginLifetimeScope(hostContainer =>
                {
                    callback.Invoke(parentScope, hostContainer);
                });
                hostAndClientLookupProcess.StoreLifetimeScope(hostScope);
            });
        }

        private static void RegisterSubSystems(this ContainerBuilder hostApplicationContainer, Topology.SubSystem[] subSystems, IPipeline pipeline)
        {
            foreach (Topology.SubSystem subSystem in subSystems)
            {
                hostApplicationContainer.CreateIsolatedScope((hostApplicationScope, componentHostContainer) =>
                {
                    ComponentHostFactory(hostApplicationScope, subSystem.EntryPoint, pipeline, HostRole.EntryPoint);
                });
                foreach (var component in subSystem.Components)
                {
                    RegisterComponentHost(hostApplicationContainer, component, pipeline);
                }
            }
        }

        private static void RegisterComponentHost(ContainerBuilder hostApplicationContainer, Topology.Host component, IPipeline pipeline)
        {
            hostApplicationContainer.CreateIsolatedScope((hostApplicationScope, componentHostContainer) =>
            {
                ComponentHostFactory(hostApplicationScope, component, pipeline);
            });
        }

        private static void ComponentHostFactory(ILifetimeScope hostApplicationScope, Topology.Host host, IPipeline pipeline, HostRole role = HostRole.Component)
        {
            var subSystemScope = hostApplicationScope.BeginLifetimeScope(componentContainer =>
            {
                if (host.ServiceCollection is not null)
                {
                    componentContainer.Populate(host.ServiceCollection);
                }
                var componentEndpointContracts = host.Endpoints.Select(m => m.Contract).ToArray();
                RegisterHost(componentContainer, host, componentEndpointContracts, pipeline, role);
                RegisterEndPoints(host, componentContainer);
                componentContainer.CreateIsolatedScope((systemScope, componentHostContainer) =>
                {
                    //register inside for InProcEndpoint
                    RegisterSubSystemHosts(componentHostContainer, host, systemScope);
                });
            });
        }

        private static void RegisterHost(ContainerBuilder componentContainer, Topology.Host component, Type[] componentEndpointContracts, IPipeline pipeline, HostRole role)
        {
            if (component is HostMock hostMock)
            {
                foreach (var componentContract in componentEndpointContracts)
                {
                    componentContainer.Register(c =>
                    {
                        List<IInterceptor> interceptors = [];
                        foreach (Type interceptorType in pipeline.ServiceInterceptors)
                        {
                            var interceptor = c.Resolve(interceptorType) as IInterceptor;
                            if (interceptor is not null)
                            {
                                interceptors.Add(interceptor);
                            }
                        }
                        return s_proxyGenerator.CreateInterfaceProxyWithTargetInterface(componentContract, hostMock.Instance, [.. interceptors]);
                    }).Named("Endpoint", componentContract);
                }
            }
            else
            {
                foreach (var cepc in componentEndpointContracts)
                {
                    componentContainer.RegisterInstance(new Role(role)).AsSelf();
                    componentContainer.RegisterType(component.Implementation).Named("Endpoint", cepc)
                    .EnableInterfaceInterceptors()
                    .InterceptedBy(pipeline.ServiceInterceptors);
                }
                RegisterProxies(componentContainer, component.Proxies);
            }
        }

        private static void RegisterSubSystemHosts(ContainerBuilder hostContainer, Topology.Host host, ILifetimeScope subSystemScope)
        {
            foreach (var endpoint in host.Endpoints)
            {
                // ISubSystemHost<I>
                Type genericSubSystemHostType = typeof(SubSystemHost<,>).MakeGenericType(host.Implementation, endpoint.Contract);
                var genericSubSystemHost = Activator.CreateInstance(genericSubSystemHostType, subSystemScope);
                Debug.Assert(genericSubSystemHost is not null);
                hostContainer.RegisterInstance(genericSubSystemHost).As(genericSubSystemHostType.GetInterfaces());
            }
        }

        private static void RegisterEndPoints(Topology.Host host, ContainerBuilder subsystemScopeBuilder)
        {
            foreach (var endpoint in host.Endpoints)
            {
                var hostChannel = endpoint.Transport.HostChannel;
                Type hostEndpointType = hostChannel?.MakeGenericType(endpoint.Contract) ?? throw new ArgumentException("Endpoint not defined for transport");

                ValidateEndpointContract(endpoint);

                // IEndpoint<I>
                subsystemScopeBuilder.RegisterType(hostEndpointType).SingleInstance().As([hostEndpointType, typeof(IEndpoint)]);
                subsystemScopeBuilder.RegisterBuildCallback(c =>
                {
                    var endpointRegister = c.Resolve<RegisteredEndpoints>();
                    var endpointInstance = (IEndpoint)c.Resolve(hostEndpointType);
                    endpointInstance.Bind(endpoint, host.Implementation.Name);
                    endpointRegister.AddEndpoint(endpointInstance);
                });
            }
        }

        private static void ValidateEndpointContract(Binding endpoint)
        {
            var groupedByName = endpoint.Contract.GetMethods().GroupBy( g=> g.Name).ToArray();
            if (groupedByName.Any(a => a.Count() != 1))
            {
                var duplicates =  groupedByName.Where(a => a.Count() > 1);
                string duplicateNames = string.Join(",", duplicates.Select(s => s.Key).ToArray());
                throw new ArgumentException($"Topology - Endpoint contract is not valid. Contains duplicate operation name: {duplicateNames}");
            }
        }

        private static void RegisterProxies(this ContainerBuilder builder, Topology.Client[] clients)
        {
            foreach (var client in clients)
            {
                builder.RegisterInstance(client);
                builder.Register(c => c.Resolve<ProxyFactory>().Create(client.Service.Contract)).As(client.Service.Contract);
            }
        }
    }
}
