using Example002.iFx.Contract;

namespace Example002.iFx.Proxy
{
    public static class Proxy
    {
        public static I ForService<I>() where I : class, IService
        {
            return SoEx.Proxy.ForService<I>();
        }

        public static I ForComponent<I>(IService service) where I : class, IService
        {
            return SoEx.Proxy.ForComponent<I>(service);
        }
    }
}
