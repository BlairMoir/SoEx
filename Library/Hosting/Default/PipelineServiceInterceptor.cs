using Castle.DynamicProxy;
using SoEx.Topology.Pipeline;

namespace SoEx.Hosting.Default
{
    public record PipelineServiceInterceptor<T> : IPipelineServiceInterceptor where T : IInterceptor
    {
        public Type ImplementationType => typeof(T);
    }
}
