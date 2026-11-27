using System.Collections.Immutable;

namespace SoEx.Topology
{
    public record SubSystem
    {
        public required Host EntryPoint { get; init; }
        public required ImmutableArray<Host> Components { get; init; }
        public required string Name { get; init; }
    }
}
