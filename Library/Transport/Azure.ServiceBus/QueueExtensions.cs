using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.Azure.ServiceBus
{
    public static class QueueExtensions
    {
        public static void SBQueueClient(this IServiceCollection collection)
        {
            collection.AddTransient(typeof(QueueChannel<>), typeof(QueueChannel<>));
        }
    }
}
