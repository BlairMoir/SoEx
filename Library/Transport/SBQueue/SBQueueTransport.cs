namespace SoEx.Transport.SBQueue
{
    public class SBQueueTransport : Topology.Transport
    {
        public SBQueueTransport()
        {
            ClientChannel = typeof(SBQueueChannel<>);
            HostChannel = typeof(SBQueueEndpoint<>);
        }
    }
}
