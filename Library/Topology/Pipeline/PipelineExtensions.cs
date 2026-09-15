namespace SoEx.Topology.Pipeline;

public static class PipelineExtensions
{
    public static Topology.System WithPipeline(this Topology.System system, IPipeline pipeline)
    {
        var replacementSystem = new Topology.System()
        {
            Clients = system.Clients, SubSystems = system.SubSystems, Defaults = pipeline
        };
        return replacementSystem;
    }
}
