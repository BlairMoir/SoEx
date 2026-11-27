using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.NATS
{
    public record NatsEventBinding<I> : Topology.Binding
    {
        public NatsEventBinding(string subSystem, NatsConfig? config = null) : base(
            typeof(I),
            new NatsEventTransport() { Address = new Address.Single(new Uri((config ?? new NatsConfig()).Url)) },
            subSystem
            )
        {
            Config = config ?? new NatsConfig();
        }

        public NatsConfig Config { get; }
    }
}
