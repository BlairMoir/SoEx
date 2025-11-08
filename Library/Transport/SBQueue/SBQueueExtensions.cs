using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.SBQueue
{
    public static class SBQueueExtensions
    {
        public static void SBQueueClient(this IServiceCollection collection)
        {
            collection.AddSingleton(typeof(SBQueueChannel<>), typeof(SBQueueChannel<>));
        }
    }
}
