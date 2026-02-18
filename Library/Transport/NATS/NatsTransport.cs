namespace SoEx.Transport.NATS
{
    public class NatsTransport : Topology.Transport
    {
        public NatsTransport()
        {
            ClientChannel = typeof(NatsChannel<>);
            HostChannel = typeof(NatsEndpoint<>);
        }
    }
}
