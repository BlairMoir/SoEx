using System.Diagnostics;

namespace SoEx.Abstractions
{
    public sealed class InvocationRequest
    {
        public required string? ActivityId { get; init; }
        public bool HasResult { get; init; }
        public required string MethodName { get; init; }
        public object?[] Arguments { get; init; } = [];
        public byte[]? AmbientContext { get; init; }
        public byte[]? FrameworkContext { get; init; }
    }
}
