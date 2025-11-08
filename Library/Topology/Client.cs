namespace SoEx.Topology
{
    public class Client<I> : Client where I : class
    {
    }

    public abstract class Client
    {
        public required string SubSystem { get; init; }
        public required Binding Service { get; init; }
    }
}
