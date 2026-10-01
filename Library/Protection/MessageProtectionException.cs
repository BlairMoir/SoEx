using System.Security.Cryptography;

namespace SoEx.Protection;


public enum ProtectionFailure
{
    Malformed,
    UnsupportedSuite,
    NotAddressed,
    UntrustedSender,
    BadSignature,
    Stale,
    Replayed,
    DecryptFailed,
    NoRecipient,
    UnexpectedMessage,
    UnknownKey,
    WrongExchange
}

public sealed class MessageProtectionException : CryptographicException
{
    public MessageProtectionException(ProtectionFailure failure)
        : base($"Message protection failed: {failure}")
    {
        Failure = failure;
    }

    public ProtectionFailure Failure { get; }
}
