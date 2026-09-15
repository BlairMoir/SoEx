using System.ComponentModel.Design;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.DependencyInjection;
using SoEx.Method.Conventions;
using SoEx.Messaging.Chimera;
using SoEx.Topology;
using SoEx.Transport.Chimera;

namespace SoExTemplate.iFx.Convention;

public static class TopologyBuilder
{
    public static SoEx.Topology.System BuildSystem(string companyName, Dictionary<Type, Action<IServiceCollection>>? scd)
    {
        var chimeraOptions = new ChimeraOptions { RootDirectory = Path.Combine(AppContext.BaseDirectory, "events") };
        SystemBuilder systemBuilder = new SystemBuilder();

        var serviceTypes = Scan.ServiceTypes(companyName);
        var managerTypes = serviceTypes.Where(w => w.Name.EndsWith(Keywords.Manager));
        var engineTypes = serviceTypes.Where(w => w.Name.EndsWith(Keywords.Engine));
        var accessTypes = serviceTypes.Where(w => w.Name.EndsWith(Keywords.Access));
        var eventTypes = Scan.ProxyTypes(companyName).Where(w => w.Name.EndsWith(Keywords.Event));

        foreach (var managerType in managerTypes)
        {
            var managerBuilder = systemBuilder.AddManager(managerType);
            foreach (var endpoint in managerType.GetInterfaces())
            {
                if (endpoint.Name.EndsWith(Keywords.Manager))
                {
                    managerBuilder.AddInProcEndpoint(endpoint);
                }
                if (endpoint.Name.EndsWith(Keywords.Event))
                {
                    var genericType = typeof(ChimeraEventBinding<>).MakeGenericType(endpoint);
                    var binding = (SoEx.Topology.Binding)Activator.CreateInstance(genericType,endpoint.Name, chimeraOptions, endpoint.Name)!;
                    managerBuilder.AddEndpoints([binding]);
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

        var topology = systemBuilder.Build();

        var eventProxies = new List<Client>();
        foreach (var eventInterface in eventTypes)
        {
            var genericType = typeof(ChimeraEventBinding<>).MakeGenericType(eventInterface);
            var binding = (SoEx.Topology.Binding)Activator.CreateInstance(genericType, eventInterface.Name, chimeraOptions, eventInterface.Name)!;
            var clientContractType = typeof(SoEx.Topology.Client<>).MakeGenericType(eventInterface);
            if (Activator.CreateInstance(clientContractType) is Client instance)
            {
                clientContractType.GetProperty(nameof(Client.Service))!.SetValue(instance, binding);
                clientContractType.GetProperty(nameof(Client.SubSystem))!.SetValue(instance, eventInterface.Name);
                eventProxies.Add(instance);
            }
        }

        var topologyWithEvents = new SoEx.Topology.System()
        {
            SubSystems = topology.SubSystems.Select(subSystem => new SubSystem()
            {
                Name = subSystem.Name,
                Components = subSystem.Components,
                EntryPoint = new SoEx.Topology.Host()
                {
                    Implementation = subSystem.EntryPoint.Implementation,
                    Endpoints = subSystem.EntryPoint.Endpoints,
                    Proxies = [..subSystem.EntryPoint.Proxies, ..eventProxies],
                    ServiceCollection = subSystem.EntryPoint.ServiceCollection
                },
            }).ToArray(),
            Clients = topology.Clients,
            Defaults = topology.Defaults,
        };

        return topologyWithEvents;
    }
}
