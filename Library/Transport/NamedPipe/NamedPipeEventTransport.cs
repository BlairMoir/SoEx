namespace SoEx.Transport.NamedPipe
{
    public record NamedPipeEventTransport : Topology.Transport
    {
        public NamedPipeEventTransport()
        {
            ClientChannel = typeof(NamedPipeEventChannel<>);
            HostChannel = typeof(NamedPipeEventEndpoint<>);
        }
    }
}
