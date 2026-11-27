using NATS.Client.Core;

namespace SoEx.Transport.NATS
{
    public class NatsConfig
    {
        public int MaxConcurrentMessages { get; init; } = 16;
        public string Url { get; init; } = NatsOpts.Default.Url;

        public NatsOpts Options { get; init; } = NatsOpts.Default;

        internal NatsOpts ClientOptions()
        {
            return Options with { Url = Url };
        }
    }
}
