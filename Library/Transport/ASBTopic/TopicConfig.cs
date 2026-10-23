namespace SoEx.Transport.ASBTopic
{
    public class TopicConfig
    {
        public required string ConnectionString { get; init; }
        public int MaxConcurrentCalls { get; init; } = 16;
    }
}
