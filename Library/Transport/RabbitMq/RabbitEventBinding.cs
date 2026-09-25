using SoEx.Topology;

namespace SoEx.Transport.RabbitMq;

public record RabbitEventBinding<I> : Binding
{
    public RabbitEventBinding(string subSystem, RabbitConfig? config = null) : base(
        typeof(I),
        new RabbitEventTransport() { Address = new Address.Single(new Uri($"soex.rabbit://{typeof(I)}")) },
        subSystem
    )
    {
        RabbitConfig = config;
    }

    public RabbitConfig? RabbitConfig { get; init; }
}
