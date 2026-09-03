using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoEx.Abstractions;
using SoEx.Context;
using SoEx.Hosting;
using SoEx.Method.Conventions;
using SoEx.Topology;
using SoEx.Transport.InProc;
using SoEx.Transport.NamedPipe;
using SoExTemplate.iFx.Convention;
using SoExTemplate.iFx.Observability;

namespace SoExTemplate.iFx.Hosting.SubSystem;

public static class Host
{
    public static IHost NamedPipe(string[] args, Dictionary<Type, Action<IServiceCollection>>? scd = null)
    {
        var assemblyName = Assembly.GetEntryAssembly()!.FullName;
        var namespaceParts = assemblyName!.Split(".");
        string subSystem = namespaceParts[0];
        string companyName = namespaceParts[1];
        var dtoAndContextTypes = Scan.DtoAndContextTypes(companyName);

        var topology = BuildSystem(subSystem, companyName, scd);

        HostApplicationBuilder serviceHostBuilder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
        serviceHostBuilder.SoEx(topology, new KnownTypes(dtoAndContextTypes));
        serviceHostBuilder.Services.InProcClient();
        serviceHostBuilder.Services.NamedPipedClient();
        serviceHostBuilder.Services.ConfigureLogging();
        serviceHostBuilder.Services.ConfigureTelemetry();
        var contextPolicies = Scan.ContextPolicyTypes(companyName);
        foreach (var policy in contextPolicies)
        {
            serviceHostBuilder.Services.AddSingleton(typeof(IContextFlowPolicy), policy);
        }

        var serviceHost = serviceHostBuilder.Build();
        return serviceHost;
    }

    private static SoEx.Topology.System BuildSystem(string subSystem, string companyName, Dictionary<Type, Action<IServiceCollection>>? scd)
    {
        SystemBuilder systemBuilder = new SystemBuilder();

        var serviceTypes = Scan.ServiceTypes(companyName);
        var managerType = serviceTypes.Single(w => w.Name.StartsWith(subSystem) && w.Name.EndsWith("Manager"));
        var engineTypes = serviceTypes.Where(w => w.Name.EndsWith("Engine"));
        var accessTypes = serviceTypes.Where(w => w.Name.EndsWith("Access"));

        var managerBuilder = systemBuilder.AddManager(managerType);

        List<Binding> managerBindings = new List<Binding>();
        foreach (var endpoint in managerType.GetInterfaces())
        {
            if (endpoint.Name.EndsWith("Manager"))
            {
                managerBindings.Add(CreateNamedPipeBinding(subSystem, endpoint));
            }
            else if (endpoint.Name.EndsWith("Event"))
            {
                managerBindings.Add(CreateNamedPipeEventBinding(subSystem, endpoint));
            }
        }
        managerBuilder.AddEndpoints(managerBindings.ToArray());
        var eventClients = FindEventClients(companyName);
        managerBuilder.AddProxies(eventClients);

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

        return systemBuilder.Build();
    }

    private static IEnumerable<Type> FilterEvents(Type[] interfaces, Type[] serviceFacets, Type theService)
    {
        var proxies = interfaces.Where(w => w.Name.EndsWith("Event"));
        return proxies;
    }

    private static Client[] FindEventClients(string company)
    {
        var proxyTypes = Scan.ProxyTypes(company);
        var eventTypes = proxyTypes.Where(w => w.Name.EndsWith(Keywords.Event));
        List<Client> eventClients = new List<Client>();
        foreach (var ev in eventTypes)
        {
            var genericType = typeof(Client<>).MakeGenericType(ev);
            var client = (Client)Activator.CreateInstance(genericType)!;
            typeof(Client).GetProperty(nameof(Client.SubSystem))?.SetValue(client, "");
            typeof(Client).GetProperty(nameof(Client.Service))?.SetValue(client, CreateNamedPipeEventBinding("", ev));
            eventClients.Add(client);
        }
        return eventClients.ToArray();
    }

    private static Binding CreateNamedPipeBinding(string subSystem, Type facet)
    {
        var genericType = typeof(NamedPipeBinding<>).MakeGenericType(facet);
        var binding = (Binding)Activator.CreateInstance(genericType, subSystem)!;
        return binding;
    }

    private static Binding CreateNamedPipeEventBinding(string subSystem, Type facet)
    {
        var genericType = typeof(NamedPipeEventBinding<>).MakeGenericType(facet);
        var binding = (Binding)Activator.CreateInstance(genericType, subSystem)!;
        return binding;
    }
}
