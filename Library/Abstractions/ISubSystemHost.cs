using Autofac;

namespace SoEx.Abstractions
{
    public interface ISubSystemHost
    {
        public ILifetimeScope BeginRequestLifetimeScope();
    }

    public interface ISubSystemHost<I> { }
}
