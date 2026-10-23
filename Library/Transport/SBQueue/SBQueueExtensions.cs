using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.SBQueue
{
    public static class SBQueueExtensions
    {
        public static void SBQueueClient(this IServiceCollection collection)
        {
            collection.AddTransient(typeof(SBQueueChannel<>), typeof(SBQueueChannel<>));
        }
    }
}
