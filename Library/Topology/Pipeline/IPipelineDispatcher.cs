using SoEx.Abstractions;

namespace SoEx.Topology.Pipeline;

public interface IPipelineDispatcher : IPipelineType
{
}

public record PipelineDispatcher<T> : IPipelineDispatcher where T : IDispatcher
{
    public Type ImplementationType => typeof(T);
}
