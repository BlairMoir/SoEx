namespace Example001.iFx.Proxy
{
    public static class Proxy
    {
        public static I ForService<I>() where I : class
        {
            return SoEx.Proxy.ForService<I>();
        }

        public static I ForComponent<I>(object service) where I : class
        {
            return SoEx.Proxy.ForComponent<I>(service);
        }
    }
}
