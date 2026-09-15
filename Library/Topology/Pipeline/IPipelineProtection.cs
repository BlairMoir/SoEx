using SoEx.Abstractions;

namespace SoEx.Topology.Pipeline;

public interface IPipelineProtection : IPipelineType
{
}

public class PipelineProtection<T>: IPipelineProtection where T : IMessageProtection
{
    public Type ImplementationType => typeof(T);
}
