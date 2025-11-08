namespace SoEx.Abstractions
{
    public class InvocationResponse
    {
        public object? Response { get; set; }
        public byte[]? AmbientContext { get; set; }
    }
}
