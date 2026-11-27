namespace SoEx.Transport.Azure.ServiceBus
{
    public class QueueConfig : ServiceBusConfig
    {
        public required string Queue { get; init; }
    }
}
