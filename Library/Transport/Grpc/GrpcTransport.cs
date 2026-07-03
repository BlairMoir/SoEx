namespace SoEx.Transport.Grpc
{
    public class GrpcTransport : Topology.Transport
    {
        public GrpcTransport()
        {
            ClientChannel = typeof(GrpcChannel<>);
            HostChannel = typeof(GrpcEndpoint<>);
        }
    }
}
