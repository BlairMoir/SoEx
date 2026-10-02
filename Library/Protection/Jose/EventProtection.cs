using System.Globalization;
using System.Security.Cryptography;
using Jose;
using SoEx.Abstractions;
using SoEx.Abstractions.Protection;

namespace SoEx.Protection.Jose;

public sealed class EventProtection : IMessageProtection
{
    private readonly MessageSecurity.EventPolicy _policy;
    private string? _pendingReplayKey;
    private DateTimeOffset _pendingExpiry;
    private readonly MessageSecurity _security;
    private readonly IMessageReplayCache _replay;
    private readonly TimeProvider _time;

    public EventProtection(MessageSecurity security, EventProtectionOptions options, IMessageReplayCache replay, TimeProvider time)
    {
        _security = security;
        _replay = replay;
        _time = time;
        _policy = MessageSecurity.Load(options);
    }

    public ValueTask<byte[]> Protect(ReadOnlyMemory<byte> payload, MessageContext context)
    {
        if (context is ClientContext clientContext)
        {
            return new ValueTask<byte[]>(Seal(payload, _policy.Current, clientContext.MethodName));
        }

        if (_pendingReplayKey is null)
        {
            throw new MessageProtectionException(ProtectionFailure.UnexpectedMessage);
        }

        _replay.TryAdd(_pendingReplayKey, _pendingExpiry);
        _pendingReplayKey = null;
        return new ValueTask<byte[]>([]);
    }

    public ValueTask<byte[]> Unprotect(ReadOnlyMemory<byte> payload,  MessageContext context)
    {
        if (context.Role == ProtectionRole.Client)
        {
            if (!payload.IsEmpty)
            {
                throw new MessageProtectionException(ProtectionFailure.UnexpectedMessage);
            }

            return new ValueTask<byte[]>([]);
        }

        if (_pendingReplayKey is not null)
        {
            throw new MessageProtectionException(ProtectionFailure.UnexpectedMessage);
        }

        return new ValueTask<byte[]>(Open(payload.Span));
    }

    private byte[] Seal(ReadOnlyMemory<byte> body, MessageSecurity.SymmetricKey key, string action)
    {
        Dictionary<string, object> signedHeaders = JoseEnvelope.SignedHeaders(_security.SigningKeyId, action, _time);

        string signed;
        using (ECDsa signer = _security.OwnSigning())
        {
            signed = JWT.EncodeBytes(body.ToArray(), signer, JwsAlgorithm.ES256, signedHeaders);
        }

        string sealedFrame = JWT.Encode(signed, key.Key, JweAlgorithm.DIR, JweEncryption.A256GCM, extraHeaders:
            new Dictionary<string, object>() { [JoseEnvelope.Key] = KeyId(key) });
        return JoseEnvelope.Bytes(sealedFrame);
    }

    private byte[] Open(ReadOnlySpan<byte> frame)
    {
        string token = JoseEnvelope.Text(frame);
        IDictionary<string, object> outer = JoseEnvelope.Headers(token, 5);
        if (JoseEnvelope.String(outer, "alg") != "dir" || JoseEnvelope.String(outer, "enc") != "A256GCM")
        {
            throw new MessageProtectionException(ProtectionFailure.UnsupportedSuite);
        }

        MessageSecurity.SymmetricKey key = KeyFor(JoseEnvelope.String(outer, JoseEnvelope.Key));

        string signed;
        try
        {
            signed = JWT.Decode(token, key.Key, JweAlgorithm.DIR, JweEncryption.A256GCM);
        }
        catch (Exception)
        {
            throw new MessageProtectionException(ProtectionFailure.DecryptFailed);
        }

        IDictionary<string,object> inner = JoseEnvelope.Headers(signed, 3);
        if (JoseEnvelope.String(inner, "alg") != "ES256")
        {
            throw new MessageProtectionException(ProtectionFailure.UnsupportedSuite);
        }

        string? publisherKeyId = JoseEnvelope.String(inner, JoseEnvelope.Sender);
        if (publisherKeyId is null)
        {
            throw new MessageProtectionException(ProtectionFailure.Malformed);
        }

        MessageSecurity.Peer? publisher = _policy.PublisherBy(publisherKeyId);
        if (publisher is null)
        {
            throw new MessageProtectionException(ProtectionFailure.UntrustedSender);
        }

        byte[] body;
        try
        {
            using ECDsa verifier = publisher.Signing();
            body = JWT.DecodeBytes(signed, verifier, JwsAlgorithm.ES256);
        }
        catch (Exception)
        {
            throw new MessageProtectionException(ProtectionFailure.BadSignature);
        }

        DateTimeOffset sent = JoseEnvelope.Fresh(inner, _policy.Window, _time);

        string? nonce = JoseEnvelope.String(inner, JoseEnvelope.Nonce);
        if (nonce is null)
        {
            throw new MessageProtectionException(ProtectionFailure.Malformed);
        }

        string replayKey = $"E{publisherKeyId}{nonce}";
        if (_replay.Seen(replayKey))
        {
            throw new MessageProtectionException(ProtectionFailure.Replayed);
        }

        _pendingReplayKey = replayKey;
        _pendingExpiry = sent + _policy.Window;
        return body;
    }

    private MessageSecurity.SymmetricKey KeyFor(string? keyId)
    {
        if (keyId == KeyId(_policy.Current))
        {
            return _policy.Current;
        }

        if (_policy.Previous is not null && keyId == KeyId(_policy.Previous))
        {
            return _policy.Previous;
        }

        throw new MessageProtectionException(ProtectionFailure.UnknownKey);
    }

    private static string KeyId(MessageSecurity.SymmetricKey key)
    {
        return key.Id.ToString(CultureInfo.InvariantCulture);
    }
}
