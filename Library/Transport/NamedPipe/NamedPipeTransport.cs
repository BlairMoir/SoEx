namespace SoEx.Transport.NamedPipe
{
    public class NamedPipeTransport : Topology.Transport
    {
        public NamedPipeTransport()
        {
            ClientChannel = typeof(NamedPipeChannel<>);
            HostChannel = typeof(NamedPipeEndpoint<>);
        }
    }
}
