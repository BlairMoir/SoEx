using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoEx.Context;
using SoEx.Hosting;
using SoEx.Topology;
using SoEx.Transport.InProc;
using SoExTemplate.iFx.Convention;
using SoExTemplate.iFx.Observability;

namespace SoExTemplate.iFx.Hosting;
public static class Host
{
    public static InProcHostApplications InProc(string[] args, Dictionary<Type, Action<IServiceCollection>>? scd = null)
    {
        var assemblyName = Assembly.GetEntryAssembly()!.FullName;
        var namespaceParts = assemblyName!.Split(".");
        string companyName = namespaceParts[0];
        var hostTopology = TopologyBuilder.BuildSystem(companyName, scd);
        var clientTopology = BuildClients(hostTopology);
        var contextPolicies = Scan.ContextPolicyTypes(companyName);

        HostApplicationBuilder serviceHostBuilder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
        serviceHostBuilder.SoEx(hostTopology);
        HostApplicationBuilder clientHostBuilder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
        clientHostBuilder.SoEx(clientTopology);

        CommonServices(contextPolicies, serviceHostBuilder.Services, clientHostBuilder.Services);
        serviceHostBuilder.Services.ConfigureTelemetry();
        serviceHostBuilder.Services.InProcClientWithSpan(clientHostBuilder.Services);

        return new InProcHostApplications()
        {
            ServiceHost = serviceHostBuilder,
            ClientHost = clientHostBuilder
        };
    }

    private static void CommonServices(Type[] contextPolicies, params IServiceCollection[] serviceCollections)
    {

        foreach (var serviceCollection in serviceCollections)
        {
            serviceCollection.ConfigureLogging();
            foreach (var policy in contextPolicies)
            {
                serviceCollection.AddSingleton(typeof(IContextFlowPolicy), policy);
            }
        }
    }

    private static SoEx.Topology.System BuildClients(SoEx.Topology.System system)
    {
        List<SoEx.Topology.Client> clients = new List<SoEx.Topology.Client>();
        foreach (var subSystem in system.SubSystems)
        {
            var endpoints = subSystem.EntryPoint.Endpoints;
            foreach (var endpoint in endpoints)
            {
                clients.Add(ToClient(endpoint, subSystem.Name));
            }
        }
        return new SoEx.Topology.System() { SubSystems = [], Clients = clients.ToArray(), Defaults = system.Defaults };
    }

    private static SoEx.Topology.Client ToClient(Binding s, string subsystemName)
    {
        var clientContractType = typeof(SoEx.Topology.Client<>).MakeGenericType(s.Contract);
        if (Activator.CreateInstance(clientContractType) is SoEx.Topology.Client instance)
        {
            clientContractType.GetProperty(nameof(SoEx.Topology.Client.Service))!.SetValue(instance, s);
            clientContractType.GetProperty(nameof(SoEx.Topology.Client.SubSystem))!.SetValue(instance, subsystemName);
            return instance;
        }
        throw new ArgumentOutOfRangeException(nameof(instance));
    }


}
