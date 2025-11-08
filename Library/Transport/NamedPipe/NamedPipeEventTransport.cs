namespace SoEx.Transport.NamedPipe
{
    public class NamedPipeEventTransport : Topology.Transport
    {
        public NamedPipeEventTransport()
        {
            ClientChannel = typeof(NamedPipeEventChannel<>);
            HostChannel = typeof(NamedPipeEventEndpoint<>);
        }
    }
}
