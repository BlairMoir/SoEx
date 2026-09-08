using System.Security.Cryptography.X509Certificates;

namespace SoEx.Transport.Grpc.Protection;

public record GrpcCertificate : GrpcProtection
{
    public required X509Certificate2 Certificate { get; init; }
}
