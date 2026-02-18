namespace SoEx.Relay
{
    public class RelayTransport : Topology.Transport
    {
        public RelayTransport()
        {
            ClientChannel = typeof(RelayChannel<>);
            HostChannel = typeof(RelayEndpoint<>);
        }
    }
}
