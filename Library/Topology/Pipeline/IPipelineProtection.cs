using SoEx.Abstractions;

namespace SoEx.Topology.Pipeline;

public interface IPipelineProtection : IPipelineType
{
    object? Options { get; init; }
}

public record PipelineProtection<T> : IPipelineProtection where T : IMessageProtection
{
    public Type ImplementationType => typeof(T);
    public object? Options { get; init; }
}
