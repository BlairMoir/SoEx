namespace SoEx.Topology
{
    public abstract class Transport
    {
        public required Uri Address { get; init; }
        public virtual Type? ClientChannel { get; init; }
        public virtual Type? HostChannel { get; init; }
    }
}
