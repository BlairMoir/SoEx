
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoEx.Context;
using SoEx.Hosting;
using SoEx.Topology;
using SoEx.Transport.NamedPipe;
using SoExTemplate.iFx.Convention;
using SoExTemplate.iFx.Observability;

namespace SoExTemplate.iFx.Hosting.Component;

public static class Host
{
    public static IHost NamedPipe(string[] args, ServiceCollection? sc = null)
    {
        var assemblyName = Assembly.GetEntryAssembly()!.FullName;
        var namespaceParts = assemblyName!.Split(".");
        string subSystem = namespaceParts[0];
        string companyName = namespaceParts[1];

        var services = Scan.ServiceTypes(companyName);
        var interfaces = Scan.ProxyTypes(companyName);
        var dtoAndContextTypes = Scan.DtoAndContextTypes(companyName);

        if (services.Length == 0)
            throw new ArgumentException("You must include a reference to at least one service");

        if (services.Length > 1)
            throw new ArgumentException("You can only reference on service");

        var theService = services[0];
        var serviceFacets = theService.GetInterfaces();
        IEnumerable<Type> proxies = FilterProxies(interfaces, serviceFacets, theService);
        IEnumerable<Type> events = FilterEvents(interfaces, serviceFacets, theService);

        List<Binding> bindings = new List<Binding>();
        foreach (var facet in serviceFacets.Where(w => !(w.FullName?.EndsWith("Event") ?? false)))
        {
            Binding binding = CreateNamedPipeBinding(subSystem, facet);
            bindings.Add(binding);
        }
        foreach (var facet in serviceFacets.Where(w => w.FullName?.EndsWith("Event") ?? false))
        {
            Binding binding = CreateNamedPipeEventBinding(subSystem, facet);
            bindings.Add(binding);
        }

        List<Client> clients = new List<Client>();
        foreach (var proxy in proxies)
        {
            var genericType = typeof(Client<>).MakeGenericType(proxy);
            var client = (Client)Activator.CreateInstance(genericType)!;
            typeof(Client).GetProperty(nameof(Client.SubSystem))!.SetValue(client, subSystem);
            typeof(Client).GetProperty(nameof(Client.Service))!.SetValue(client, CreateNamedPipeBinding(subSystem, proxy));
            clients.Add(client);
        }
        List<Client> eventClients = new List<Client>();
        foreach (var ev in events)
        {
            var genericType = typeof(Client<>).MakeGenericType(ev);
            var client = (Client)Activator.CreateInstance(genericType)!;
            typeof(Client).GetProperty(nameof(Client.SubSystem))!.SetValue(client, subSystem);
            typeof(Client).GetProperty(nameof(Client.Service))!.SetValue(client, CreateNamedPipeEventBinding(subSystem, ev));
            eventClients.Add(client);
        }

        SoEx.Topology.Host host = new SoEx.Topology.Host()
        {
            Implementation = theService,
            Endpoints = [.. bindings],
            Proxies = [.. clients, .. eventClients],
            ServiceCollection = sc
        };


        HostApplicationBuilder serviceHostBuilder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
        serviceHostBuilder.SoEx(host);
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

    private static IEnumerable<Type> FilterProxies(Type[] interfaces, Type[] serviceFacets, Type theService)
    {
        var proxies = interfaces
            .Except(serviceFacets)
            .Where(w => !w.Name.EndsWith("Manager"))
            .Where(w => !w.Name.EndsWith("Event"));
        return proxies;
    }

    private static IEnumerable<Type> FilterEvents(Type[] interfaces, Type[] serviceFacets, Type theService)
    {
        var proxies = interfaces.Where(w => w.Name.EndsWith("Event"));
        return proxies;
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
