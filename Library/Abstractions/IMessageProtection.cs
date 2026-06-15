namespace SoEx.Abstractions
{
    public interface IMessageProtection
    {
        ValueTask<byte[]> Protect(ReadOnlyMemory<byte> payload);
        ValueTask<byte[]> Unprotect(ReadOnlyMemory<byte> payload);
    }
}
