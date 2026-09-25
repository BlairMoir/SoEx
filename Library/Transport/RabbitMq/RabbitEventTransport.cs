namespace SoEx.Transport.RabbitMq;

public record RabbitEventTransport : Topology.Transport
{
    public RabbitEventTransport()
    {
        ClientChannel = typeof(RabbitEventChannel<>);
        HostChannel = typeof(RabbitEventEndpoint<>);
    }
}
