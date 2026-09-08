using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.NamedPipe
{
    public class NamedPipeBinding<I> : Topology.Binding
    {
        [SetsRequiredMembers]
        public NamedPipeBinding(string subSystem)
        {
            Contract = typeof(I);
            SubSystem = subSystem;
            Transport = new NamedPipeTransport() { Address = new Address.Single(new Uri($"soex.namedpipe://{SubSystem}-{typeof(I)}")) };
        }
    }
}
