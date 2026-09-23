namespace SoEx.Transport.Chimera;

public record ChimeraEventTransport : Topology.Transport
{
    public ChimeraEventTransport()
    {
        ClientChannel = typeof(ChimeraEventChannel<>);
        HostChannel = typeof(ChimeraEventEndpoint<>);
    }
}
