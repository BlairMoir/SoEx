using SoEx.Abstractions;

namespace SoEx.Hosting.Default
{
    public class DevelopmentConfidentiality : ITelemetryConfidentiality
    {
        public object Protect<T>(T message, string? key = null)
        {
            if(message is null)
                return string.Empty;

            return message;
        }
    }
}
