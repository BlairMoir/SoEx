using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoEx.PubSub.InProc;

namespace SoEx.PubSub
{
    public static class PubSubInProcExtentions
    {
        public static IHostApplicationBuilder WithPubSub(this IHostApplicationBuilder hostBuilder)
        {
            AddServices(hostBuilder.Services);
            return hostBuilder;
        }

        public static IHostBuilder WithPubSub(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(AddServices);
        }

        private static void AddServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<PubSubChannel>();
            serviceCollection.AddTransient(typeof(IPublishInterceptor<>),typeof(PublishInterceptor<>));
            serviceCollection.AddHostedService<SubscribeListener>();
        }
    }
}