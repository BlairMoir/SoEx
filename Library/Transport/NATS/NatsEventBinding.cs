using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.NATS
{
    public record NatsEventBinding<I> : Topology.Binding
    {
        public NatsEventBinding(string subSystem) : base(
            typeof(I),
            new NatsEventTransport() { Address = new Address.Single(new Uri($"soex.nats://{typeof(I)}")) },
            subSystem
            )
        {
        }
    }
}
