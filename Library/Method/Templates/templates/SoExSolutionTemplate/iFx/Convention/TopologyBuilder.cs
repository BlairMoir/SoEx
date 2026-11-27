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
    public static SoEx.Topology.System BuildSystem(string companyName, Dictionary<Type, Action<IServiceCollection>>? scd, params Type[] utilitySubsystems)
    {
        var chimeraOptions = new ChimeraOptions { RootDirectory = Path.Combine(AppContext.BaseDirectory, "events") };

        var serviceTypes = Scan.ServiceTypes(companyName);
        var managerTypes = serviceTypes.Where(w => w.Name.EndsWith(Keywords.Manager));
        var engineTypes = serviceTypes.Where(w => w.Name.EndsWith(Keywords.Engine));
        var accessTypes = serviceTypes.Where(w => w.Name.EndsWith(Keywords.Access));
        var utilityTypes = serviceTypes.Where(w => w.Name.EndsWith(Keywords.Utility)).ToArray();
        var eventTypes = Scan.ProxyTypes(companyName).Where(w => w.Name.EndsWith(Keywords.Event));

        SystemBuilder systemBuilder = new SystemBuilder()
            .WithEvents(contract => contract.ToChimeraBinding(chimeraOptions))
            .AddEvents([.. eventTypes]);

        foreach (var utility in utilityTypes)
        {
            if (!utilitySubsystems.Contains(utility))
                continue;

            var utilityBuilder = systemBuilder.AddUtility(utility);
            foreach (var uc in utility.GetInterfaces())
            {
                utilityBuilder.AddInProcEndpoint(uc);
            }
            if( scd is not null && scd.ContainsKey(utility))
            {
                utilityBuilder.ConfigureServices(scd[utility]);
            }
        }

        foreach (var managerType in managerTypes)
        {
            var managerBuilder = systemBuilder.AddManager(managerType);
            foreach (var endpoint in managerType.GetInterfaces())
            {
                if(!endpoint.Name.EndsWith(Keywords.Manager))
                    continue;

                managerBuilder.AddInProcEndpoint(endpoint);
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
            foreach (var utility in utilityTypes)
            {
                if (utilitySubsystems.Contains(utility))
                    continue;

                var utilityBuilder = managerBuilder.AddUtility(utility);
                foreach (var uc in utility.GetInterfaces())
                {
                    utilityBuilder.AddInProcEndpoint(uc);
                }
                if( scd is not null && scd.ContainsKey(utility))
                {
                    utilityBuilder.ConfigureServices(scd[utility]);
                }
            }
        }

        var topology = systemBuilder.Build();
        return topology;
    }
}
