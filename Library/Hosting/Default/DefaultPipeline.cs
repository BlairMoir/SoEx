using SoEx.Topology;

namespace SoEx.Hosting.Default
{
    public class DefaultPipeline : IPipeline
    {
        public Type Dispatcher => typeof(DefaultDispatcher);
        public Type TelemeteryConfidentiality => typeof(FallbackConfidentiality);
        public Type MessageProtection => typeof(NullProtection);
        public Type MessageSerializer => typeof(SoEx.Hosting.Serializers.NewtonsoftJson.JsonMessageSerializer);
        public Type[] ServiceInterceptors => [typeof(ErrorInterceptor)];
    }
}
