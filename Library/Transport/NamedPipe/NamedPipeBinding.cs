using System.Diagnostics.CodeAnalysis;

namespace SoEx.Transport.NamedPipe
{
    public class NamedPipeBinding<I> : Topology.Binding
    {
        [SetsRequiredMembers]
        public NamedPipeBinding(string subSystem)
        {
            Contract = typeof(I);
            SubSystem = subSystem;
            Transport = new NamedPipeTransport() { Address = new Uri($"soex.namedpipe://{SubSystem}-{typeof(I)}") };
        }
    }
}
