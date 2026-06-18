namespace SoEx.Abstractions
{
    public interface IMessageProtection
    {
        ValueTask<byte[]> Protect(ReadOnlyMemory<byte> payload, string? methodName = null);
        ValueTask<byte[]> Unprotect(ReadOnlyMemory<byte> payload);
    }
}
