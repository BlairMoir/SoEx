using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.NATS
{
    public class NatsEventBinding<I> : Topology.Binding
    {
        [SetsRequiredMembers]
        public NatsEventBinding(string subSystem)
        {
            Contract = typeof(I);
            SubSystem = subSystem;
            Transport = new NatsEventTransport() { Address = new Address.Single(new Uri($"soex.nats://{typeof(I)}")) };
        }
    }
}
