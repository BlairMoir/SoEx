using SoEx.Topology;
using SoEx.Topology.Pipeline;

namespace SoEx.Hosting.Default
{
    public class DefaultPipeline : IPipeline
    {
        public IPipelineDispatcher Dispatcher { get; init; } = new PipelineDispatcher<DefaultDispatcher>();

        public IPipelineConfidentiality TelemetryConfidentiality { get; init; } =
            new PipelineConfidentiality<FallbackConfidentiality>();

        public IPipelineProtection MessageProtection { get; init; } = new PipelineProtection<NullProtection>();

        public IPipelineSerializer MessageSerializer { get; init; } =
            new PipelineSerializer<SoEx.Hosting.Serializers.NewtonsoftJson.OpenJsonMessageSerializer>();
        public Type[] ServiceInterceptors => [typeof(ErrorInterceptor)];
    }
}
