namespace SoEx.Transport.Grpc.Protection;

public class GrpcCertificateFromPath : GrpcProtection
{
    public required string CertPath { get; init; }
    public required string CertPassword { get; init; }
}
