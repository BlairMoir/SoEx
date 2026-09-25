namespace SoEx.Transport.RabbitMq;

public class RabbitConfig
{
    public string? UserName { get; init; }
    public string? Password { get; init; }
    public string? VirtualHost { get; init; }
    public string? HostName { get; init; }
}
