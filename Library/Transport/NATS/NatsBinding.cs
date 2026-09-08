using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.NATS
{
    public class NatsBinding<I> : Topology.Binding
    {
        [SetsRequiredMembers]
        public NatsBinding(string subSystem)
        {
            Contract = typeof(I);
            SubSystem = subSystem;
            Transport = new NatsTransport() { Address = new Address.Single(new Uri($"soex.nats://{SubSystem}-{typeof(I)}")) };
        }
    }
}
