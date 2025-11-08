namespace SoEx.Topology
{
    public abstract class Binding
    {
        public required Type Contract { get; init; }
        public required Transport Transport { get; init; }
        public required string SubSystem { get; init; }
        public IPipeline? Pipeline { get; set; }
    }
}
