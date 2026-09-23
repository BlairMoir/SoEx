namespace SoEx.Transport.ASBTopic
{
    public record ASBTopicEventTransport : Topology.Transport
    {
        public ASBTopicEventTransport()
        {
            ClientChannel = typeof(ASBTopicEventChannel<>);
            HostChannel = typeof(ASBTopicEventEndpoint<>);
        }
    }
}
