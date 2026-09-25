namespace SoEx.Method.Conventions;

public class MethodSubSystem
{
    public required string Name { get; init; }
    public Topology.Host? EntryPoint { get; set; }
    public MethodComponent[] Engines { get; set; } = [];
    public MethodComponent[] Access { get; set; } = [];
    public MethodComponent[] Utilities { get; set; } = [];
}
