namespace SoEx.Topology
{
    public record Client<I> : Client where I : class
    {
    }

    public abstract record Client
    {
        public required string SubSystem { get; init; }
        public required Binding Service { get; init; }
    }
}
