namespace SoEx
{
    public static class Proxy
    {
        public static I ForService<I>() where I : class
        {
            return CreateProxy<I>();
        }

        public static I ForComponent<I>(object service) where I : class
        {
            ArgumentNullException.ThrowIfNull(service);
            return CreateProxy<I>();
        }
        private static I CreateProxy<I>() where I : class
        {
            return Container.Resolve<I>();
        }
    }
}
