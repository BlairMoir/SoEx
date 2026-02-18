using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.ThreadChannel
{
    public static class UnsafeThreadChannelExtensions
    {
        public static void ThreadChannelClient(this IServiceCollection collection)
        {
            collection.AddTransient(typeof(UnsafeThreadChannelChannel<>), typeof(UnsafeThreadChannelChannel<>));
            collection.AddSingleton(typeof(UnsafeThreadEventChannel<>), typeof(UnsafeThreadEventChannel<>));
        }
        public static void InProcHost() { }
    }
}
