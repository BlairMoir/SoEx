using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.NamedPipe
{
    public class NamedPipeEventBinding<I> : Topology.Binding
    {
        [SetsRequiredMembers]
        public NamedPipeEventBinding(string subSystem)
        {
            Contract = typeof(I);
            SubSystem = subSystem;
            Transport = new NamedPipeEventTransport() { Address = new Address.Single(new Uri($"soex.namedpipe://{typeof(I)}")) };
        }
    }
}
