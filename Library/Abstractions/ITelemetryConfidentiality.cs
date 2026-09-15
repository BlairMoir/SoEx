namespace SoEx.Abstractions
{
    public interface ITelemetryConfidentiality
    {
        object Protect<T>(T message, string? key = null);
    }
}
