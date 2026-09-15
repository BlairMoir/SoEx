namespace SoEx.Transport.Chimera;

public class ChimeraEventTransport : Topology.Transport
{
    public ChimeraEventTransport()
    {
        ClientChannel = typeof(ChimeraEventChannel<>);
        HostChannel = typeof(ChimeraEventEndpoint<>);
    }
}
