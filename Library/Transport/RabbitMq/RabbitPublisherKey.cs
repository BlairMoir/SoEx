using RabbitMQ.AMQP.Client;

namespace SoEx.Transport.RabbitMq
{
    internal sealed record RabbitPublisherKey(string Hosts, Func<ConnectionSettingsBuilder, ConnectionSettingsBuilder>? Connection);
}
