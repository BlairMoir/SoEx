using System.Diagnostics;

namespace SoEx.Abstractions
{
    public class InvocationRequest
    {
        public required string? ActivityId { get; set; }
        public bool HasResult { get; set; }
        public required string MethodName { get; set; }
        public object?[] Arguments { get; set; } = [];
        public byte[]? AmbientContext { get; set; }
        public byte[]? FrameworkContext { get; set; }
    }
}
