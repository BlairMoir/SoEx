namespace SoEx.Transport.Azure.ServiceBus
{
    public record QueueTransport : Topology.Transport
    {
        public QueueTransport()
        {
            ClientChannel = typeof(QueueChannel<>);
            HostChannel = typeof(QueueEndpoint<>);
        }
    }
}
