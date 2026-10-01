using System.Security.Cryptography;
using Jose;
using SoEx.Abstractions;
using SoEx.Abstractions.Protection;

namespace SoEx.Protection;

public sealed class RequestProtection : IMessageProtection
{
    private readonly MessageSecurity.RequestPolicy _policy;
    private MessageSecurity.Peer? _awaitingReplyFrom;
    private string? _requestNonce;
    private OpenedMessage? _request;
    private readonly MessageSecurity _security;
    private readonly IMessageReplayCache _replay;
    private readonly TimeProvider _time;

    public RequestProtection(MessageSecurity security, RequestProtectionOptions options, IMessageReplayCache replay, TimeProvider time)
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
            byte[] sealedRequest = SealRequest(payload, clientContext.MethodName);
            return new ValueTask<byte[]>(sealedRequest);
        }

        byte[] sealedReply = SealReply(payload);
        return new ValueTask<byte[]>(sealedReply);
    }

    public ValueTask<byte[]> Unprotect(ReadOnlyMemory<byte> payload, MessageContext context)
    {
        if (context.Role == ProtectionRole.Client)
        {
            byte[] openedReply = OpenReply(payload.Span);
            return new ValueTask<byte[]>(openedReply);
        }

        byte[] openedRequest = OpenRequest(payload.Span);
        return new ValueTask<byte[]>(openedRequest);
    }

    private byte[] SealRequest(ReadOnlyMemory<byte> body, string methodName)
    {
        MessageSecurity.Peer? peer = _policy.Receipient ?? throw new MessageProtectionException(ProtectionFailure.NoRecipient);
        Dictionary<string, object> headers = JoseEnvelope.SignedHeaders(_security.SigningKeyId, methodName, _time);
        _awaitingReplyFrom = peer;
        _requestNonce = (string)headers[JoseEnvelope.Nonce];
        return Seal(body, peer, headers);
    }

    private byte[] SealReply(ReadOnlyMemory<byte> body)
    {
        OpenedMessage request = _request ?? throw new MessageProtectionException(ProtectionFailure.UnexpectedMessage);
        Dictionary<string,object> headers = JoseEnvelope.SignedHeaders(_security.SigningKeyId, null, _time);
        headers[JoseEnvelope.InReplyTo] = request.Nonce;
        return Seal(body, request.Sender, headers);
    }

    private byte[] OpenReply(ReadOnlySpan<byte> envelope)
    {
        if (_awaitingReplyFrom is null)
        {
            throw new MessageProtectionException(ProtectionFailure.UnexpectedMessage);
        }

        if (envelope.IsEmpty)
        {
            throw new MessageProtectionException(ProtectionFailure.Malformed);
        }

        OpenedMessage reply = Open(envelope, ExpectedReplier, false);
        return reply.Body;
    }

    private byte[] OpenRequest(ReadOnlySpan<byte> envelope)
    {
        if (_request is not null)
        {
            throw new MessageProtectionException(ProtectionFailure.UnexpectedMessage);
        }

        _request = Open(envelope, _policy.CallerBy, true);
        return _request.Body;
    }

    private MessageSecurity.Peer? ExpectedReplier(string senderKeyId)
    {
        if (_awaitingReplyFrom is not null && senderKeyId == _awaitingReplyFrom.SigningKeyId)
        {
            return _awaitingReplyFrom;
        }

        return null;
    }

    private byte[] Seal(ReadOnlyMemory<byte> body, MessageSecurity.Peer recipient, Dictionary<string, object> signedHeaders)
    {
        signedHeaders[JoseEnvelope.To] = recipient.EncryptionKeyId;

        string signed;
        using (ECDsa signer = _security.OwnSigning())
        {
            signed = JWT.EncodeBytes(body.ToArray(), signer, JwsAlgorithm.ES256, signedHeaders);
        }

        using ECDiffieHellman recipientKey = recipient.Encryption();
        string sealedFrame = JWT.Encode(signed, recipientKey, JweAlgorithm.ECDH_ES, JweEncryption.A256GCM, extraHeaders:
            new Dictionary<string, object>()
            {
                [JoseEnvelope.Key] = recipient.EncryptionKeyId
            });

        return JoseEnvelope.Bytes(sealedFrame);
    }

    private OpenedMessage Open(ReadOnlySpan<byte> frame, Func<string,MessageSecurity.Peer?> senderLookup, bool isRequest)
    {
        string token = JoseEnvelope.Text(frame);
        IDictionary<string, object> outer = JoseEnvelope.Headers(token, 5);
        if (JoseEnvelope.String(outer, "alg") != "ECDH-ES" || JoseEnvelope.String(outer, "enc") != "A256GCM")
        {
            throw new MessageProtectionException(ProtectionFailure.UnsupportedSuite);
        }

        if (JoseEnvelope.String(outer, JoseEnvelope.Key) != _security.EncryptionKeyId)
        {
            throw new MessageProtectionException(ProtectionFailure.NotAddressed);
        }

        string signed;
        try
        {
            using ECDiffieHellman own = _security.OwnEncryption();
            signed = JWT.Decode(token, own, JweAlgorithm.ECDH_ES, JweEncryption.A256GCM);
        }catch(Exception)
        {
            throw new MessageProtectionException(ProtectionFailure.DecryptFailed);
        }

        IDictionary<string, object> inner = JoseEnvelope.Headers(signed, 3);
        if (JoseEnvelope.String(inner, "alg") != "ES256")
        {
            throw new MessageProtectionException(ProtectionFailure.UnsupportedSuite);
        }

        string? senderKeyId = JoseEnvelope.String(inner, JoseEnvelope.Sender);
        if (senderKeyId is null)
        {
            throw new MessageProtectionException(ProtectionFailure.Malformed);
        }

        MessageSecurity.Peer? sender = senderLookup(senderKeyId);
        if (sender is null)
        {
            throw new MessageProtectionException(ProtectionFailure.UntrustedSender);
        }

        byte[] body;
        try
        {
            using ECDsa verifier = sender.Signing();
            body = JWT.DecodeBytes(signed, verifier, JwsAlgorithm.ES256);
        }catch(Exception)
        {
            throw new MessageProtectionException(ProtectionFailure.BadSignature);
        }

        if (JoseEnvelope.String(inner, JoseEnvelope.To) != _security.EncryptionKeyId)
        {
            throw new MessageProtectionException(ProtectionFailure.NotAddressed);
        }

        bool hasAction = JoseEnvelope.String(inner, JoseEnvelope.Action) is not null;
        if (hasAction != isRequest)
        {
            throw new MessageProtectionException(ProtectionFailure.WrongExchange);
        }

        if (!isRequest && _policy.BindsReplies && JoseEnvelope.String(inner, JoseEnvelope.InReplyTo) != _requestNonce)
        {
            throw new MessageProtectionException(ProtectionFailure.WrongExchange);
        }

        DateTimeOffset sent = JoseEnvelope.Fresh(inner, _policy.Window, _time);
        string? nonce = JoseEnvelope.String(inner, JoseEnvelope.Nonce);
        if (nonce is null)
        {
            throw new MessageProtectionException(ProtectionFailure.Malformed);
        }

        if (!_replay.TryAdd($"R{_security.EncryptionKeyId}{senderKeyId}{nonce}", sent + _policy.Window))
        {
            throw new MessageProtectionException(ProtectionFailure.Replayed);
        }

        return new OpenedMessage(body, sender, nonce);
    }

    private sealed class OpenedMessage(byte[] body, MessageSecurity.Peer sender, string nonce)
    {
        public byte[] Body { get; } = body;
        public MessageSecurity.Peer Sender { get; } = sender;
        public string Nonce { get; } = nonce;
    }
}
