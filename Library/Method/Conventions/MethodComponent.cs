namespace SoEx.Method.Conventions;

public class MethodComponent
{
    public required string SubSystem { get; init; }
    public required Topology.Host Host { get; set; }
}
