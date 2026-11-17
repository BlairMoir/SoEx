using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoEx.Abstractions;
using SoEx.Hosting.Default;
using SoEx.Topology;

namespace SoEx.Hosting
{
    public static class HostExtensions
    {
        public static IHostApplicationBuilder SoEx(this IHostApplicationBuilder hostApplicationBuilder, Topology.System systemTopology)
        {
            return SoEx(hostApplicationBuilder, systemTopology, new KnownTypes());
        }

        public static IHostApplicationBuilder SoEx(this IHostApplicationBuilder hostApplicationBuilder, Topology.System systemTopology, KnownTypes knownTypes)
        {
            hostApplicationBuilder.ConfigureContainer(
                new AutofacServiceProviderFactory(hostApplicationContainer =>
                    {
                        hostApplicationContainer.RegisterSoEx(systemTopology, knownTypes);
                    }
                ));
            return hostApplicationBuilder;
        }

        public static IHostApplicationBuilder SoEx(this IHostApplicationBuilder hostApplicationBuilder, Topology.Host hostTopology, IPipeline? pipeline = null)
        {
            return SoEx(hostApplicationBuilder, hostTopology, new KnownTypes(), pipeline);
        }

        public static IHostApplicationBuilder SoEx(this IHostApplicationBuilder hostApplicationBuilder, Topology.Host hostTopology, KnownTypes knownTypes, IPipeline? pipeline = null)
        {
            hostApplicationBuilder.ConfigureContainer(
                new AutofacServiceProviderFactory(hostApplicationContainer =>
                    {
                        hostApplicationContainer.RegisterSoEx(hostTopology, knownTypes, pipeline);
                    }
            ));
            return hostApplicationBuilder;
        }

        public static ContainerBuilder RegisterSoEx(this ContainerBuilder hostApplicationContainer, Topology.System systemTopology, KnownTypes knownTypes)
        {
            hostApplicationContainer.RegisterBindings(systemTopology, knownTypes);
            hostApplicationContainer.RegisterSystem(systemTopology, knownTypes);
            return hostApplicationContainer;
        }

        public static ContainerBuilder RegisterSoEx(this ContainerBuilder hostApplicationContainer, Topology.Host hostTopology, KnownTypes knownTypes, IPipeline? pipeline)
        {
            var componentPipeline = pipeline ?? new DefaultPipeline();
            hostApplicationContainer.RegisterBindings(hostTopology, knownTypes, pipeline);
            hostApplicationContainer.RegisterComponent(hostTopology, knownTypes, componentPipeline);
            return hostApplicationContainer;
        }
    }
}
