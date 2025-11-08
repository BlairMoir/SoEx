using System.Runtime.Serialization.Json;
using SoEx.Abstractions;
using SoEx.Hosting;
using SoEx.Topology;

namespace SoEx.Hosting
{
    public class DefaultPipeline : IPipeline
    {
        public Type Dispatcher => typeof(DefaultDispatcher);
        public Type MessageSerializer => typeof(SoEx.Hosting.Serializers.NewtonsoftJson.JsonMessageSerializer);
        public Type[] ServiceInterceptors => [typeof(ErrorInterceptor)];
    }
}
