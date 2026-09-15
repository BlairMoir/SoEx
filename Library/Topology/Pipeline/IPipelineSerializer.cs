using SoEx.Abstractions;

namespace SoEx.Topology.Pipeline;

public interface IPipelineSerializer : IPipelineType
{

}

public class PipelineSerializer<T> : IPipelineSerializer  where T : IMessageSerializer
{
    public Type ImplementationType => typeof(T);
}
