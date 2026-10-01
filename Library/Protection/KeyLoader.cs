using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SoEx.Protection;

internal static class KeyLoader
{
    public static X509Certificate2 Certificate(KeyConfig config, bool needsPrivateKey)
    {
        X509Certificate2 certificate;
        if (config is CertificateFile file && needsPrivateKey)
        {
            certificate = X509CertificateLoader.LoadPkcs12FromFile(file.Path, file.Password);
        }else if (config is CertificateFile publicFile)
        {
            certificate = X509CertificateLoader.LoadCertificateFromFile(publicFile.Path);
        }
        else if(config is CertificateStore store)
        {
            certificate = FromStore(store);
        }
        else
        {
            throw new ArgumentException($"{config} does not refer to a certificate");
        }

        ValidateCertificate(config, needsPrivateKey, certificate);

        return certificate;
    }

    private static void ValidateCertificate(KeyConfig config, bool needsPrivateKey, X509Certificate2 certificate)
    {
        if (needsPrivateKey && !certificate.HasPrivateKey)
        {
            throw new ArgumentException($"{config} has no private key");
        }

        using ECDsa? key = certificate.GetECDsaPublicKey();
        if (key is null || key.ExportParameters(includePrivateParameters: false).Curve.Oid.Value !=
            ECCurve.NamedCurves.nistP256.Oid.Value)
        {
            throw new ArgumentException($"{config} is not a NIST P-256 key");
        }
    }

    public static byte[] SymmetricKey(KeyConfig config)
    {
        if (config is not KeyFile file)
        {
            throw new ArgumentException($"{config} is not a key file");
        }
        byte[] key;
        try
        {
            key = Convert.FromBase64String(File.ReadAllText(file.Path).Trim());
        }
        catch (FormatException)
        {
            throw new ArgumentException($"{config} is not formatted correctly. expected base64");
        }

        if (key.Length != 32)
        {
            CryptographicOperations.ZeroMemory(key);
            throw new ArgumentException($"{config} must hold a 256-bit key");
        }

        return key;
    }

    private static X509Certificate2 FromStore(CertificateStore reference)
    {
        using(var store = new X509Store(reference.Name, reference.Location))
        {
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection found =
                store.Certificates.Find(X509FindType.FindByThumbprint, reference.Thumbprint, validOnly: false);
            if (found.Count != 1)
            {
                throw new ArgumentException($"{reference} matched {found.Count} certificates");
            }
            return found[0];
        }
    }
}
