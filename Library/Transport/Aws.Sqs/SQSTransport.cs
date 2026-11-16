namespace SoEx.Transport.Aws.Sqs
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
