using System.Security.Cryptography.X509Certificates;
using SoEx.Transport.Grpc.Protection;


namespace SoEx.Transport.Grpc
{
    public class GrpcConfig
    {
        public string? BindAddress { get; init; }
        public required string Host { get; init; }
        public required int Port { get; init; }
        public Protection.GrpcProtection Protection { get; init; } = new ClearTextGrpc();

    }
}
