using SoEx.Topology.Pipeline;

namespace SoEx.Topology;

public interface IBindingPipeline
{
    public IPipelineSerializer MessageSerializer { get; }
    public IPipelineProtection MessageProtection { get; }
    public IPipelineDispatcher Dispatcher { get; }
}
