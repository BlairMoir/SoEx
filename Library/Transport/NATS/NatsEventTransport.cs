namespace SoEx.Transport.NATS
{
    public record NatsEventTransport : Topology.Transport
    {
        public NatsEventTransport()
        {
            ClientChannel = typeof(NatsEventChannel<>);
            HostChannel = typeof(NatsEventEndpoint<>);
        }
    }
}
