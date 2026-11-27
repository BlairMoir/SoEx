using Microsoft.Azure.Relay;

namespace SoEx.Transport.Azure.Relay
{
    public class RelayConfig
    {
        public required string RelayNamespace { get; init; }
        public required string ConnectionName { get; init; }
        public string? KeyName { get; init; }
        public string? Key { get; init; }
        public TokenProvider? TokenProvider { get; init; }
    }
}
