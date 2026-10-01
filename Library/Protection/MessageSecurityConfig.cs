using System.Collections.Immutable;

namespace SoEx.Protection;

public class MessageSecurityConfig
{
    public required KeyConfig EncryptionKey { get; init; }
    public required KeyConfig SigningKey { get; init; }
}

public sealed record TrustedPeer(KeyConfig Signing, KeyConfig? Encryption = null);

public sealed record RequestProtectionOptions
{
    public TrustedPeer? Recipient { get; init; }
    public ImmutableArray<TrustedPeer> Callers { get; init; } = [];
    public TimeSpan FreshnessWindow { get; init; } = TimeSpan.FromMinutes(5);
    public bool BindRepliesToRequests { get; init; }
}

public sealed record EventProtectionOptions
{
    public required EventKey Current { get; init; }
    public EventKey? Previous { get; init; }
    public ImmutableArray<TrustedPeer> Publishers { get; init; } = [];
    public TimeSpan FreshnessWindow { get; init; } = TimeSpan.FromHours(1);
}

public sealed record EventKey(byte Id, KeyConfig Key);
