using System.Diagnostics.CodeAnalysis;

namespace SoEx.Transport.InProc
{
    public class InProcBinding<I> : Topology.Binding
    {
        [SetsRequiredMembers]
        public InProcBinding(string subSystem)
        {
            Contract = typeof(I);
            SubSystem = subSystem;
            Transport = new InProcTransport() { Address = new Uri($"soex.inproc://{SubSystem}-{typeof(I)}") };
        }
    }
}
