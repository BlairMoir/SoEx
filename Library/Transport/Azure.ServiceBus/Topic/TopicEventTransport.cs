namespace SoEx.Transport.Azure.ServiceBus
{
    public record TopicEventTransport : Topology.Transport
    {
        public TopicEventTransport()
        {
            ClientChannel = typeof(TopicEventChannel<>);
            HostChannel = typeof(TopicEventEndpoint<>);
        }
    }
}
