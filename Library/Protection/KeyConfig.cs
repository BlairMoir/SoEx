using System.Security.Cryptography.X509Certificates;

namespace SoEx.Protection;

public abstract record KeyConfig;

public sealed record CertificateFile(string Path, string? Password = null) : KeyConfig
{
    public override string ToString() => $"Certificate file: {Path}";
}
public sealed record CertificateStore(string Thumbprint,
    StoreLocation Location = StoreLocation.CurrentUser, StoreName Name = StoreName.My) : KeyConfig;
public sealed record KeyFile(string Path) : KeyConfig;
