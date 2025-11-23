using SoEx.Topology;
namespace SoExTemplate.iFx.Client;

public class ClientBuilder
{
    private List<SoEx.Topology.Client> _clients = new List<SoEx.Topology.Client>();

    public void AddClientFor<I>(Type bindingType,  string subSystem) where I : class
    {
        var genericType = bindingType.MakeGenericType(typeof(I));
        var binding = (Binding)Activator.CreateInstance(genericType, subSystem)!;
        _clients.Add(new SoEx.Topology.Client<I>()
        {
            Service = binding, SubSystem = subSystem
        });
    }

    public SoEx.Topology.System Build()
    {
        return new SoEx.Topology.System()
        {
            Clients = _clients.ToArray(),
            SubSystems = []
        };
    }
}
