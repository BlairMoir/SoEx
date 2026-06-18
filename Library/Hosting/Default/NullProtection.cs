using SoEx.Abstractions;

namespace SoEx.Hosting.Default
{
    public class NullProtection : IMessageProtection
    {
        public ValueTask<byte[]> Protect(ReadOnlyMemory<byte> payload, string? methodName) => ValueTask.FromResult(payload.ToArray());
        public ValueTask<byte[]> Unprotect(ReadOnlyMemory<byte> payload) => ValueTask.FromResult(payload.ToArray());
    }
}
