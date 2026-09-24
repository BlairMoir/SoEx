using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.SBQueue
{
    public record SBQueueBinding<I> : Topology.Binding
    {
        private readonly SBConfig _config;
        public SBQueueBinding(SBConfig config) : base(
            typeof(I),
            new SBQueueTransport() { Address = new Address.Single(new Uri($"sb://{config.SBNamespace}.servicebus.windows.net/")) },
            "NotSet"
            )
        {
            _config = config;
        }

        public SBConfig Config => _config;
    }
}
