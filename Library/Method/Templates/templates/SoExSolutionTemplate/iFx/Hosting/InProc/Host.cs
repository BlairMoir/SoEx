using System.Diagnostics.Tracing;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoEx.Abstractions;
using SoEx.Context;
using SoEx.Hosting;
using SoEx.Method.Conventions;
using SoEx.Topology;
using SoEx.Transport.InProc;
using SoEx.Transport.Chimera;
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
        var dtoAndContextTypes = Scan.DtoAndContextTypes(companyName);
        var clientTopology = BuildClients(hostTopology);
        var contextPolicies = Scan.ContextPolicyTypes(companyName);

        HostApplicationBuilder serviceHostBuilder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
        serviceHostBuilder.SoEx(hostTopology, new KnownTypes(dtoAndContextTypes));
        HostApplicationBuilder clientHostBuilder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
        clientHostBuilder.SoEx(clientTopology, new KnownTypes(dtoAndContextTypes));

        CommonServices(contextPolicies, serviceHostBuilder.Services, clientHostBuilder.Services);
        serviceHostBuilder.Services.ConfigureTelemetry();
        serviceHostBuilder.Services.InProcClientWithSpan(clientHostBuilder.Services);
        serviceHostBuilder.Services.ChimeraClient();

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
                if (endpoint.Contract.Name.EndsWith(Keywords.Event))
                    continue;

                clients.Add(endpoint.ToClient(subSystem.Name));
            }
        }
        return new SoEx.Topology.System() { SubSystems = [], Clients = [..clients], Defaults = system.Defaults };
    }
}
