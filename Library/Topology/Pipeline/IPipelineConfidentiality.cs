using SoEx.Abstractions;

namespace SoEx.Topology.Pipeline;

public interface IPipelineConfidentiality : IPipelineType
{
}

public class PipelineConfidentiality<T> : IPipelineConfidentiality where T : ITelemetryConfidentiality
{
    public Type ImplementationType => typeof(T);
}
