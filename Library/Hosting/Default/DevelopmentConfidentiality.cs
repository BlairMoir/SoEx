using SoEx.Abstractions;

namespace SoEx.Hosting.Default
{
    public class DevelopmentConfidentiality : ITelemetryConfidentiality
    {
        public object Protect<T>(T message, string? key = null) => $"{message}";
    }
}
