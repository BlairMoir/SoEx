namespace SoEx.Transport.ThreadChannel
{
    public class UnsafeThreadChannelTransport : Topology.Transport
    {
        public UnsafeThreadChannelTransport()
        {
            ClientChannel = typeof(UnsafeThreadChannelChannel<>);
            HostChannel = typeof(UnsafeThreadChannelEndpoint<>);
        }
    }
}
