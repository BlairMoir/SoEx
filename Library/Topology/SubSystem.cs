using System.Reflection.Metadata.Ecma335;

namespace SoEx.Topology
{
    public class SubSystem
    {
        public required Host EntryPoint { get; init; }
        public required Host[] Components { get; init; }
        public required string Name { get; init; }
    }
}
