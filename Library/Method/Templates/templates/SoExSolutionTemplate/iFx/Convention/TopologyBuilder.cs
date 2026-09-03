using Microsoft.Extensions.DependencyInjection;
using SoEx.Method.Conventions;

namespace SoExTemplate.iFx.Convention;

public static class TopologyBuilder
{
    public static SoEx.Topology.System BuildSystem(string companyName, Dictionary<Type, Action<IServiceCollection>>? scd)
    {
        SystemBuilder systemBuilder = new SystemBuilder();

        var serviceTypes = Scan.ServiceTypes(companyName);
        var dtoAndContextTypes = Scan.DtoAndContextTypes(companyName);
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
}
