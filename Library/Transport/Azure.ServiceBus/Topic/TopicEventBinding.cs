using SoEx.Topology;

namespace SoEx.Transport.Azure.ServiceBus
{
    public record TopicEventBinding<I> : Topology.Binding
    {
        private readonly TopicConfig _topicConfig;

        public TopicEventBinding(string subsystem, TopicConfig config) : base(
            typeof(I),
            new TopicEventTransport() { Address = new Address.Single(new Uri($"sb://{config.Namespace}/")) },
            subsystem
            )
        {
            _topicConfig = config;
        }

        public TopicConfig Config => _topicConfig;
    }
}
