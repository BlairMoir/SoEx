namespace SoEx.Transport.RabbitMq;

public class RabbitConfig
{
    public required string Host { get; init; }
    public int Port { get; init; } = 5672;
    public string? UserName { get; init; }
    public string? Password { get; init; }
    public string? VirtualHost { get; init; }
}
