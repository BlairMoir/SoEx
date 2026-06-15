using SoEx.Abstractions;

namespace SoEx.Hosting.Default
{
    public class DevelopmentConfidentiality : ITelemetryConfidentiality
    {
        public string Protect(string message) => message;
        public string Protect<T>(T message) => $"{message}";
    }
}
