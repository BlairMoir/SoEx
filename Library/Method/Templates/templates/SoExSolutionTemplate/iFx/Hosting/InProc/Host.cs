using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using SoEx.Context;
using SoEx.Hosting;
using SoEx.Method.Conventions;
using SoEx.Topology;
using SoEx.Transport.InProc;

namespace SoExTemplate.iFx.Hosting;
public static class Host
{
    public static InProcHostApplications InProc(string[] args, Dictionary<Type, Action<IServiceCollection>>? scd = null)
    {
        var assemblyName = Assembly.GetEntryAssembly()!.FullName;
        var namespaceParts = assemblyName!.Split(".");
        string companyName = namespaceParts[0];
        var hostTopology = BuildSystem(companyName, scd);
        var clientTopology = BuildClients(hostTopology);
        var contextPolicies = ContextPolicyTypes(companyName);

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

    public static IServiceCollection ConfigureTelemetry(this IServiceCollection services)
    {
        string? hostAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        Debug.Assert(hostAssemblyName is not null);
        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService(hostAssemblyName).Build()
            )
            .WithTracing(tracing => tracing
                .AddSource(SoEx.Diagnostics.ActivitySourceNames.Client)
                .AddSource(SoEx.Diagnostics.ActivitySourceNames.Host)
                .AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri($"http://localhost:5341/ingest/otlp/v1/traces");
                    exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
            );
        return services;
    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection services)
    {
        string? hostAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithProperty(nameof(hostAssemblyName), hostAssemblyName)
            .WriteTo.Seq("http://localhost:5341")
            .WriteTo.Console());
        return services;
    }

    private static SoEx.Topology.System BuildSystem(string companyName, Dictionary<Type, Action<IServiceCollection>>? scd)
    {
        SystemBuilder systemBuilder = new SystemBuilder();

        var serviceTypes = FindServiceTypes(companyName);
        var managerTypes = serviceTypes.Where(w => w.Name.EndsWith("Manager"));
        var engineTypes = serviceTypes.Where(w => w.Name.EndsWith("Engine"));
        var accessTypes = serviceTypes.Where(w => w.Name.EndsWith("Access"));

        foreach (var managerType in managerTypes)
        {
            var managerBuilder = systemBuilder.AddManager(managerType);
            foreach (var endpoint in managerType.GetInterfaces())
            {
                if (endpoint.Name.EndsWith("Manager"))
                {
                    managerBuilder.AddInProcEndpoint(endpoint);
                }
            }
            foreach (var engine in engineTypes)
            {
                var engineBuilder = managerBuilder.AddEngine(engine);
                foreach (var ec in engine.GetInterfaces())
                {
                    engineBuilder.AddInProcEndpoint(ec);
                }
                if (scd is not null && scd.ContainsKey(engine))
                {
                    engineBuilder.ConfigureServices(scd[engine]);
                }
            }
            foreach (var access in accessTypes)
            {
                var accessBuilder = managerBuilder.AddAccess(access);
                foreach (var ac in access.GetInterfaces())
                {
                    accessBuilder.AddInProcEndpoint(ac);
                }
                if (scd is not null && scd.ContainsKey(access))
                {
                    accessBuilder.ConfigureServices(scd[access]);
                }
            }
        }
        return systemBuilder.Build();
    }

    private static SoEx.Topology.System BuildClients(SoEx.Topology.System system)
    {
        List<Client> clients = new List<Client>();
        foreach (var subSystem in system.SubSystems)
        {
            var endpoints = subSystem.EntryPoint.Endpoints;
            foreach (var endpoint in endpoints)
            {
                clients.Add(ToClient(endpoint, subSystem.Name));
            }
        }
        return new SoEx.Topology.System() { SubSystems = [], Clients = clients.ToArray() };
    }

    private static Client ToClient(Binding s, string subsystemName)
    {
        var clientContractType = typeof(SoEx.Topology.Client<>).MakeGenericType(s.Contract);
        if (Activator.CreateInstance(clientContractType) is Client instance)
        {
            clientContractType.GetProperty(nameof(Client.Service))!.SetValue(instance, s);
            clientContractType.GetProperty(nameof(Client.SubSystem))!.SetValue(instance, subsystemName);
            return instance;
        }
        throw new ArgumentOutOfRangeException(nameof(instance));
    }

    private static Type[] FindServiceTypes(string company)
    {
        string[] serviceSuffixConventionKeywords = ["Manager", "Engine", "Access", "Utility"];
        List<Type> foundTypes = [];

        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyFiles = Directory.GetFiles(path!, $"{company}.*.Service.dll", SearchOption.TopDirectoryOnly);
        foreach (var assemblyFile in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var types = assembly.GetTypes().Where(t => serviceSuffixConventionKeywords.Any(s => t.Name.EndsWith(s)));
            foundTypes.AddRange(types);

        }
        return foundTypes.ToArray();
    }

    private static Type[] ContextPolicyTypes(string company)
    {
        List<Type> foundTypes = [];

        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyFiles = Directory.GetFiles(path!, $"{company}.*.Policy.dll", SearchOption.TopDirectoryOnly);
        foreach (var assemblyFile in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var types = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IContextFlowPolicy)));
            foundTypes.AddRange(types);
        }
        return foundTypes.ToArray();
    }
}
