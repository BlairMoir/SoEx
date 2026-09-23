namespace SoEx.Transport.ThreadChannel
{
    public record UnsafeThreadChannelTransport : Topology.Transport
    {
        public UnsafeThreadChannelTransport()
        {
            ClientChannel = typeof(UnsafeThreadChannelChannel<>);
            HostChannel = typeof(UnsafeThreadChannelEndpoint<>);
        }
    }
}
