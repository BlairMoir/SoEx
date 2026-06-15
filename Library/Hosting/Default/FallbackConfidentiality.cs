using SoEx.Abstractions;

namespace SoEx.Hosting.Default
{
    public class FallbackConfidentiality : ITelemetryConfidentiality
    {
        public string Protect(string message) => "[redacted]";
        public string Protect<T>(T message) => $"[redacted:{message?.GetType().Name ?? typeof(T).Name}])]";
    }
}
