using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.NATS
{
    public record NatsBinding<I> : Topology.Binding
    {
        public NatsBinding(string subSystem, NatsConfig? config = null) : base(
            typeof(I),
            new NatsTransport() { Address = new Address.Single(new Uri((config ?? new NatsConfig()).Url)) },
            subSystem
            )
        {
            Config = config ?? new NatsConfig();
        }

        public NatsConfig Config { get; }
    }
}
