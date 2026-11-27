using SoEx.Abstractions;
using SoEx.Abstractions.Protection;

namespace SoEx.Hosting.Default
{
    public class NullProtection : IMessageProtection
    {
        public ValueTask<byte[]> Protect(ReadOnlyMemory<byte> payload, MessageContext context) => ValueTask.FromResult(payload.ToArray());
        public ValueTask<byte[]> Unprotect(ReadOnlyMemory<byte> payload, MessageContext context) => ValueTask.FromResult(payload.ToArray());
    }
}
