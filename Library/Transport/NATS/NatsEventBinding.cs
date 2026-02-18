using System.Diagnostics.CodeAnalysis;

namespace SoEx.Transport.NATS
{
    public class NatsEventBinding<I> : Topology.Binding
    {
        [SetsRequiredMembers]
        public NatsEventBinding(string subSystem)
        {
            Contract = typeof(I);
            SubSystem = subSystem;
            Transport = new NatsEventTransport() { Address = new Uri($"soex.nats://{typeof(I)}") };
        }
    }
}
