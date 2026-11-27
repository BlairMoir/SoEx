using SoEx.Abstractions;

namespace SoEx.Hosting.Default
{
    public class FallbackConfidentiality : ITelemetryConfidentiality
    {
        public object Protect<T>(T message, string? key = null) => $"[redacted:{message?.GetType().Name ?? typeof(T).Name}]";
    }
}
