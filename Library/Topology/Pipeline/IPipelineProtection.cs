using SoEx.Abstractions;

namespace SoEx.Topology.Pipeline;

public interface IPipelineProtection : IPipelineType
{
}

public record PipelineProtection<T>: IPipelineProtection where T : IMessageProtection
{
    public Type ImplementationType => typeof(T);
}
