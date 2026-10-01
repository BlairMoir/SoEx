namespace SoEx.Abstractions.Protection;

public class MessageContext
{
    public required Type Contract { get; init; }
    public ProtectionRole Role { get; init; } = ProtectionRole.Server;
}
