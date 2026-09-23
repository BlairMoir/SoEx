namespace SoEx.Transport.SBQueue
{
    public record SBQueueTransport : Topology.Transport
    {
        public SBQueueTransport()
        {
            ClientChannel = typeof(SBQueueChannel<>);
            HostChannel = typeof(SBQueueEndpoint<>);
        }
    }
}
