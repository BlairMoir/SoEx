using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SoEx.Protection;

public class MessageSecurity
{
    private static readonly ConditionalWeakTable<RequestProtectionOptions, RequestPolicy> s_requests = new();
    private static readonly ConditionalWeakTable<EventProtectionOptions, EventPolicy> s_events = new();

    private readonly X509Certificate2 _encryption;
    private readonly X509Certificate2 _signing;

    public MessageSecurity(MessageSecurityConfig config)
    {
        _encryption = KeyLoader.Certificate(config.EncryptionKey, needsPrivateKey: true);
        _signing = KeyLoader.Certificate(config.SigningKey, needsPrivateKey: true);
        EncryptionKeyId = KeyId(_encryption.PublicKey.ExportSubjectPublicKeyInfo());
        SigningKeyId = KeyId(_signing.PublicKey.ExportSubjectPublicKeyInfo());
    }

    internal string EncryptionKeyId { get; }
    internal string SigningKeyId { get; }

    internal ECDiffieHellman OwnEncryption() => _encryption.GetECDiffieHellmanPrivateKey() ??
                                               throw new CryptographicException(
                                                   "The encryption certificate has no usable ECDH private key");
    internal ECDsa OwnSigning() => _signing.GetECDsaPrivateKey() ?? throw new CryptographicException("The signing certificate has no usable ECDSA private key");

    internal static RequestPolicy Load(RequestProtectionOptions options) =>
        s_requests.GetValue(options, RequestPolicy.From);
    internal static EventPolicy Load(EventProtectionOptions options) =>
        s_events.GetValue(options, EventPolicy.From);

    internal static string KeyId(ReadOnlySpan<byte> subjectPublicKeyInfo)
    {
        return Convert.ToHexString(SHA256.HashData(subjectPublicKeyInfo)[..16]);
    }

    private static Dictionary<string, Peer> BySigningKeyId(IEnumerable<TrustedPeer> peers)
    {
        var bySigningKeyId = new Dictionary<string, Peer>();

        foreach (var trusted in peers)
        {
            var peer = Peer.From(trusted);
            if (!bySigningKeyId.TryAdd(peer.SigningKeyId, peer))
            {
                throw new ArgumentException($"Two peers share a signing key ({trusted.Signing})");
            }
        }

        return bySigningKeyId;
    }

    internal sealed class RequestPolicy(Peer? recipient, Dictionary<string, Peer> callers, TimeSpan window, bool bindsReplies)
    {
        public Peer? Recipient { get; } =  recipient;
        public TimeSpan Window  { get; } = window;
        public bool BindsReplies { get; } = bindsReplies;

        public Peer? CallerBy(string signingKeyId)
        {
            return callers.GetValueOrDefault(signingKeyId);
        }

        public static RequestPolicy From(RequestProtectionOptions options)
        {
            Peer? recipient = null;
            if (options.Recipient is not null)
            {
                recipient = Peer.From(options.Recipient);
            }

            return new  RequestPolicy(recipient, BySigningKeyId(options.Callers), options.FreshnessWindow, options.BindRepliesToRequests);
        }
    }

    internal sealed record SymmetricKey(byte Id, byte[] Key);

    internal sealed class EventPolicy(SymmetricKey current, SymmetricKey? previous, Dictionary<string, Peer> publishers, TimeSpan window)
    {
        public SymmetricKey Current { get; } = current;
        public SymmetricKey? Previous { get; } = previous;
        public TimeSpan Window  { get; } = window;

        public Peer? PublisherBy(string signingKeyId)
        {
            return publishers.GetValueOrDefault(signingKeyId);
        }

        public static EventPolicy From(EventProtectionOptions options)
        {
            if (options.Previous?.Id == options.Current.Id)
            {
                throw new ArgumentException("The previous event key must have a different id");
            }

            var current = new SymmetricKey(options.Current.Id, KeyLoader.SymmetricKey(options.Current.Key));

            SymmetricKey? previous = null;
            if (options.Previous is not null)
            {
                previous = new SymmetricKey(options.Previous.Id, KeyLoader.SymmetricKey(options.Previous.Key));
            }
            return new EventPolicy(current, previous, BySigningKeyId(options.Publishers), options.FreshnessWindow);
        }
    }

    internal sealed class Peer
    {
        private readonly byte[] _encryptionSpki;
        private readonly byte[] _signingSpki;

        private Peer(byte[] encryptionSpki, byte[] signingSpki)
        {
            _encryptionSpki = encryptionSpki;
            _signingSpki = signingSpki;
            EncryptionKeyId = KeyId(_encryptionSpki);
            SigningKeyId = KeyId(_signingSpki);
        }

        public string EncryptionKeyId { get; init; }
        public string SigningKeyId { get; init; }

        public ECDiffieHellman Encryption()
        {
            ECDiffieHellman key = ECDiffieHellman.Create();
            key.ImportSubjectPublicKeyInfo(_encryptionSpki, out _);
            return key;
        }

        public ECDsa Signing()
        {
            ECDsa key  = ECDsa.Create();
            key.ImportSubjectPublicKeyInfo(_signingSpki, out _);
            return key;
        }

        public static Peer From(TrustedPeer trusted)
        {
            using X509Certificate2 signing = KeyLoader.Certificate(trusted.Signing, false);
            KeyConfig keyConfig = trusted.Encryption ?? trusted.Signing;
            using X509Certificate2 encryptionCert = KeyLoader.Certificate(keyConfig, false);
            return new Peer(encryptionCert.PublicKey.ExportSubjectPublicKeyInfo(), signing.PublicKey.ExportSubjectPublicKeyInfo());
        }
    }
}
