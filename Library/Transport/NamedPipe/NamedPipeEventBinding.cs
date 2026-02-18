using System.Diagnostics.CodeAnalysis;

namespace SoEx.Transport.NamedPipe
{
    public class NamedPipeEventBinding<I> : Topology.Binding
    {
        [SetsRequiredMembers]
        public NamedPipeEventBinding(string subSystem)
        {
            Contract = typeof(I);
            SubSystem = subSystem;
            Transport = new NamedPipeEventTransport() { Address = new Uri($"soex.namedpipe://{typeof(I)}") };
        }
    }
}
