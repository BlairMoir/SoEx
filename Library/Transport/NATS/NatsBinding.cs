using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.NATS
{
    public record NatsBinding<I> : Topology.Binding
    {
        public NatsBinding(string subSystem) : base(
            typeof(I),
            new NatsTransport() { Address = new Address.Single(new Uri($"soex.nats://{subSystem}-{typeof(I)}")) },
            subSystem
            )
        {
        }
    }
}
