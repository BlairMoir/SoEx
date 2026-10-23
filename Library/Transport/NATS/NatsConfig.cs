using NATS.Client.Core;

namespace SoEx.Transport.NATS;

public class NatsConfig
{
    public string Url { get; init; } = NatsOpts.Default.Url;
}
