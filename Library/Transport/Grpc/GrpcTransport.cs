namespace SoEx.Transport.Grpc
{
    public record GrpcTransport : Topology.Transport
    {
        public GrpcTransport()
        {
            ClientChannel = typeof(GrpcChannel<>);
            HostChannel = typeof(GrpcEndpoint<>);
        }
    }
}
