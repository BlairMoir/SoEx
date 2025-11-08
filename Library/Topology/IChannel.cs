using SoEx.Abstractions;

namespace SoEx.Topology
{
    public interface IChannel
    {
        public Task<byte[]> InvokeResult(byte[] invocationRequest);
        public void Bind(Binding binding);

        public IPipeline? Pipeline { get; }
    }
}
