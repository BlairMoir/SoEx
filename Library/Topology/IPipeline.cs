using System.Collections.Immutable;
using SoEx.Topology.Pipeline;

namespace SoEx.Topology
{
    public interface IPipeline : IBindingPipeline
    {

        public IPipelineConfidentiality TelemetryConfidentiality { get; }

        public ImmutableArray<IPipelineServiceInterceptor> ServiceInterceptors { get; }
    }
}
