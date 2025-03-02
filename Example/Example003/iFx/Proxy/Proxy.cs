namespace Example003.iFx.Proxy
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

        public static I ForEvent<I>(object service) where I : class
        {
            return SoEx.PubSub.Publish.Event<I>(service);
        }
    }
}
