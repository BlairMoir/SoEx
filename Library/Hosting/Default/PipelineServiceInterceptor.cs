using Castle.DynamicProxy;
using SoEx.Topology.Pipeline;

namespace SoEx.Hosting.Default;

public class PipelineServiceInterceptor<T> : IPipelineServiceInterceptor where T: IInterceptor
{
    public Type ImplementationType => typeof(T);
}
