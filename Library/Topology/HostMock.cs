namespace SoEx.Topology
{
    public record HostMock : Host
    {
        public required object Instance { get; init; }
    }
}
