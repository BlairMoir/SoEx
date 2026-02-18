using System.Diagnostics.CodeAnalysis;

namespace SoEx.Transport.ASBTopic
{
    public class ASBTopicEventBinding<I> : Topology.Binding
    {
        private readonly TopicConfig _topicConfig;

        [SetsRequiredMembers]
        public ASBTopicEventBinding(string subsystem, TopicConfig config)
        {
            SubSystem = subsystem;
            Contract = typeof(I);
            Transport = new ASBTopicEventTransport() { Address = new Uri($"soex.nats://{typeof(I)}") };
            _topicConfig = config;
        }

        public TopicConfig Config => _topicConfig;
    }
}
