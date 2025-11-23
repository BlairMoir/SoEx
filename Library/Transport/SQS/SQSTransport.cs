namespace SoEx.Transport.SQS
{
    public class SQSTransport : Topology.Transport
    {
        public SQSTransport()
        {
            ClientChannel = typeof(SQSChannel<>);
            HostChannel = typeof(SQSEndpoint<>);
        }
    }
}
