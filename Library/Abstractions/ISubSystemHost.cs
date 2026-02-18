using Autofac;

namespace SoEx.Abstractions
{
    public interface ISubSystemHost
    {
        public ILifetimeScope BeginRequestLifetimeScope();
    }

    public interface ISubSystemHostChannel
    {
        public C Resolve<C>() where C : class;
    }
    public interface ISubSystemHost<I> { }
}
