using System.Collections.Immutable;

namespace SoEx.Topology
{
    public record System
    {
        public required ImmutableArray<SubSystem> SubSystems { get; init; }
        public required ImmutableArray<Client> Clients { get; init; }
        public IPipeline? Defaults { get; init; }
    }
}
