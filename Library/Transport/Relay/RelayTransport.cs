namespace SoEx.Relay
{
    public record RelayTransport : Topology.Transport
    {
        public RelayTransport()
        {
            ClientChannel = typeof(RelayChannel<>);
            HostChannel = typeof(RelayEndpoint<>);
        }
    }
}
