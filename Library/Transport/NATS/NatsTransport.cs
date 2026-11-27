namespace SoEx.Transport.NATS
{
    public record NatsTransport : Topology.Transport
    {
        public NatsTransport()
        {
            ClientChannel = typeof(NatsChannel<>);
            HostChannel = typeof(NatsEndpoint<>);
        }
    }
}
