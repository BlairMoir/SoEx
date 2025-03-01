using Castle.DynamicProxy;

namespace SoEx.PubSub {

    public static class Publish
    {
        private static readonly ProxyGenerator s_proxyGenerator = new ProxyGenerator();
        public static I Event<I>(object o) where I : class
        {
            I proxy = s_proxyGenerator.CreateInterfaceProxyWithoutTarget<I>(Container.Resolve<IPublishInterceptor<I>>());
            return proxy;
        }
    }
}
