using System.Collections.Immutable;
using RabbitMQ.AMQP.Client;
using SoEx.Topology;

namespace SoEx.Transport.RabbitMq
{
    public record RabbitEventBinding<I> : Binding
    {
        public RabbitEventBinding(string subSystem, ImmutableArray<RabbitConfig> config) : base(
            typeof(I),
            new RabbitEventTransport() { Address = TransportAddress(config) },
            subSystem
        )
        {
            RabbitConfig = config;
        }

        public ImmutableArray<RabbitConfig> RabbitConfig { get; }
        public Func<ConnectionSettingsBuilder, ConnectionSettingsBuilder>? Connection { get; init; }

        private static Address TransportAddress(ImmutableArray<RabbitConfig> config)
        {
            if (config.IsDefaultOrEmpty)
            {
                throw new ArgumentException("At least one RabbitMQ host is required", nameof(config));
            }

            List<Uri> uriList = new List<Uri>();
            foreach (RabbitConfig host in config)
            {
                string virtualHost = Uri.EscapeDataString(host.VirtualHost ?? "/");
                string scheme = "amqp";
                if (host.UseTLS)
                {
                    scheme = "amqps";
                }
                var uri = new Uri($"{scheme}://{host.Host}:{host.Port}/{virtualHost}");
                uriList.Add(uri);
            }

            return new Address.Many([.. uriList]);
        }
    }
}
