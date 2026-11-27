namespace SoEx.Abstractions
{
    public sealed class InvocationResponse
    {
        public object? Response { get; init; }
        public byte[]? AmbientContext { get; init; }
    }
}
