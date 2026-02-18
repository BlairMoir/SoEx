namespace SoEx.Transport.ASBTopic
{
    public class ASBTopicEventTransport : Topology.Transport
    {
        public ASBTopicEventTransport()
        {
            ClientChannel = typeof(ASBTopicEventChannel<>);
            HostChannel = typeof(ASBTopicEventEndpoint<>);
        }
    }
}
