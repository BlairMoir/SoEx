using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.InProc
{
    public class InProcBinding<I> : Topology.Binding
    {
        [SetsRequiredMembers]
        public InProcBinding(string subSystem)
        {
            Contract = typeof(I);
            SubSystem = subSystem;
            Transport = new InProcTransport() { Address = new Address.Single(new Uri($"soex.inproc://{SubSystem}-{typeof(I)}")) };
        }
    }
}
