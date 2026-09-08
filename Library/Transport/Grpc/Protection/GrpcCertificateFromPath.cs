namespace SoEx.Transport.Grpc.Protection;

public record GrpcCertificateFromPath : GrpcProtection
{
    public required string CertPath { get; init; }
    public required string CertPassword { get; init; }
}
