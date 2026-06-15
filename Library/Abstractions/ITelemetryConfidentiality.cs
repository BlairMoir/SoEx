namespace SoEx.Abstractions
{
    public interface ITelemetryConfidentiality
    {
        string Protect(string message);
        string Protect<T>(T message);
    }
}
