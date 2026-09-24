using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.NamedPipe
{
    public record NamedPipeBinding<I> : Topology.Binding
    {
        public NamedPipeBinding(string subSystem) : base(
            typeof(I),
            new NamedPipeTransport() { Address = new Address.Single(new Uri($"soex.namedpipe://{subSystem}-{typeof(I)}")) },
            subSystem
            )
        {}
    }
}
