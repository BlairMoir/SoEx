using System.Collections.Immutable;
using SoEx.Topology;

namespace SoEx.Transport.RabbitMq;

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

    private static Address TransportAddress(ImmutableArray<RabbitConfig> config)
    {
        if (config.IsDefaultOrEmpty)
        {
            throw new ArgumentException("At least one RabbitMQ host is required", nameof(config));
        }

        List<Uri> uriList = new List<Uri>();
        foreach (var host in config)
        {
            var virtualHost = Uri.EscapeDataString(host.VirtualHost ?? "/");
            var uri = new Uri($"amqp://{host.Host}:{host.Port}/{virtualHost}");
            uriList.Add(uri);
        }

        return new Address.Many([.. uriList]);
    }
}
