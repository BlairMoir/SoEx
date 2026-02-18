using System.Diagnostics.CodeAnalysis;

namespace SoEx.Transport.ThreadChannel
{
    public class UnsafeThreadChannelBinding<I> : Topology.Binding
    {
        [SetsRequiredMembers]
        public UnsafeThreadChannelBinding()
        {
            Contract = typeof(I);
            SubSystem = "NotSet";
            Transport = new UnsafeThreadChannelTransport() { Address = new Uri($"soex.channel://{typeof(I)}") };
        }
    }
}
