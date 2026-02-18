namespace SoEx.Topology
{
    public class System
    {
        public required SubSystem[] SubSystems { get; init; }
        public required Client[] Clients { get; init; }
        public IPipeline? Defaults { get; init; }
    }
}
