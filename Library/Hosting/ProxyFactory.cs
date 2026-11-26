using Castle.DynamicProxy;

namespace SoEx.Hosting
{
    public class ProxyFactory
    {
        private static readonly ProxyGenerator s_proxyGenerator = new ProxyGenerator();
        private readonly Func<Type, ProxyInterceptor> _proxyInterceptor;

        public ProxyFactory(Func<Type, ProxyInterceptor> proxyInterceptor)
        {
            _proxyInterceptor = proxyInterceptor;
        }

        public object Create(Type serviceInterface)
        {
            return s_proxyGenerator.CreateInterfaceProxyWithoutTarget(serviceInterface, _proxyInterceptor(serviceInterface));
        }
    }
}
