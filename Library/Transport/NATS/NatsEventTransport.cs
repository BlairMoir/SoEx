namespace SoEx.Transport.NATS
{
    public class NatsEventTransport : Topology.Transport
    {
        public NatsEventTransport()
        {
            ClientChannel = typeof(NatsEventChannel<>);
            HostChannel = typeof(NatsEventEndpoint<>);
        }
    }
}
