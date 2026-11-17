using SoEx.Topology;

namespace SoEx.Transport.Azure.ServiceBus
{
    public record QueueBinding<I> : Topology.Binding
    {
        private readonly QueueConfig _config;
        public QueueBinding(QueueConfig config) : base(
            typeof(I),
            new QueueTransport() { Address = new Address.Single(new Uri($"sb://{config.Namespace}/")) },
            "NotSet"
            )
        {
            _config = config;
        }

        public QueueConfig Config => _config;
    }
}
