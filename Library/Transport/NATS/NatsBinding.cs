using System.Diagnostics.CodeAnalysis;

namespace SoEx.Transport.NATS
{
    public class NatsBinding<I> : Topology.Binding
    {
        [SetsRequiredMembers]
        public NatsBinding(string subSystem)
        {
            Contract = typeof(I);
            SubSystem = subSystem;
            Transport = new NatsTransport() { Address = new Uri($"soex.nats://{SubSystem}-{typeof(I)}") };
        }
    }
}
