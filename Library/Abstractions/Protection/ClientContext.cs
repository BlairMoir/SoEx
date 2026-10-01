namespace SoEx.Abstractions.Protection;

public class ClientContext : MessageContext
{
    public ClientContext()
    {
        Role = ProtectionRole.Client;
    }
    public required string MethodName { get; init; }
}
