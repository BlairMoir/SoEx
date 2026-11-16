namespace SoEx.Transport.Azure.ServiceBus
{
    public class Config
    {
        public required string ConnectionString { get; init; }
        public required string SBNamespace { get; init; }
        public required string Queue { get; init; }
        public int MaxConcurrentCalls { get; init; } = 16;
    }
}
