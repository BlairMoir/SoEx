using SoEx;
using SoEx.Context;

namespace SoExTemplate.iFx.Service
{
    public static class Context<T> where T : class
    {
        public static T Data => Container.Resolve<IAmbientContext>().Get<T>();
        public static void SetContext(T context) => Container.Resolve<IAmbientContext>().SetIfNotExists(() => context);
    }
}
