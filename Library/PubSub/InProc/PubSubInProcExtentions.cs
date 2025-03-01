using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoEx.PubSub.InProc;

namespace SoEx.PubSub
{
    public static class PubSubInProcExtentions
    {
        public static IHostBuilder WithPubSub(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(  services => {
                services.AddSingleton<PubSubChannel>();
                services.AddTransient(typeof(IPublishInterceptor<>),typeof(PublishInterceptor<>));
                services.AddHostedService<SubscribeListener>();
            } );
        }
    }
}