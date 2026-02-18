namespace SoEx.Transport.SBQueue
{
    public class SBConfig
    {
        public required string ConnectionString { get; init; }
        public required string SBNamespace { get; init; }
        public required string Queue { get; init; }
    }
}
