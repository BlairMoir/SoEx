using SoEx.Context;

namespace Example101.iFx.Service
{
    public static class Context<T> where T : class
    {
        public static T Data => SoEx.Container.Resolve<IAmbientContext>().Get<T>();
        public static void SetContext(T context) => SoEx.Container.Resolve<IAmbientContext>().SetIfNotExists(() => context);

    }
}
