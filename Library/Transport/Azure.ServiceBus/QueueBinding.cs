using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.Azure.ServiceBus
{
    public record QueueBinding<I> : Topology.Binding
    {
        private readonly Config _config;
        public QueueBinding(Config config) : base(
            typeof(I),
            new QueueTransport() { Address = new Address.Single(new Uri($"sb://{config.SBNamespace}.servicebus.windows.net/")) },
            "NotSet"
            )
        {
            _config = config;
        }

        public Config Config => _config;
    }
}
