namespace SoEx.Topology
{
    public abstract record Binding
    {
        public required Type Contract { get; init; }
        public required Transport Transport { get; init; }
        public required string SubSystem { get; init; }
        public IBindingPipeline? Pipeline { get; set; }
    }
}
