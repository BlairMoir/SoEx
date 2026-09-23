namespace SoEx.Topology
{
    public interface IChannel
    {
        public Task<byte[]> InvokeResult(byte[] invocationRequest);
        public void Bind(Binding binding);

        public IBindingPipeline? Pipeline { get; }
        public Type Contract { get; }
    }
}
