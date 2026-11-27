using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.Azure.ServiceBus
{
    public static class TopicExtensions
    {
        public static IServiceCollection TopicClient(this IServiceCollection collection)
        {
            collection.AddTransient(typeof(TopicEventChannel<>), typeof(TopicEventChannel<>));
            return collection;
        }
    }
}
