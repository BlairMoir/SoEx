namespace SoEx.Transport.SQS
{
    public record SQSTransport : Topology.Transport
    {
        public SQSTransport()
        {
            ClientChannel = typeof(SQSChannel<>);
            HostChannel = typeof(SQSEndpoint<>);
        }
    }
}
