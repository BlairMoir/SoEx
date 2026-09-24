using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.NamedPipe
{
    public record NamedPipeEventBinding<I> : Topology.Binding
    {
        public NamedPipeEventBinding(string subSystem) : base(
            typeof(I),
            new NamedPipeEventTransport() { Address = new Address.Single(new Uri($"soex.namedpipe://{typeof(I)}")) },
            subSystem
            )
        {
        }
    }
}
