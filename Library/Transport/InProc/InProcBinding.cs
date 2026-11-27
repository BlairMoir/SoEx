using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.InProc
{
    public record InProcBinding<I> : Topology.Binding
    {
        public InProcBinding(string subSystem) : base(
            typeof(I),
            new InProcTransport() { Address = new Address.Single(new Uri($"soex.inproc://{subSystem}-{typeof(I)}")) },
            subSystem
            )
        {
        }
    }
}
