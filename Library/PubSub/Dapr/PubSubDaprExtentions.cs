using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoEx.PubSub.Dapr;

namespace SoEx.PubSub
{
    public static class PubSubDaprExtentions
    {
        public static IHostApplicationBuilder WithPubSub(this IHostApplicationBuilder hostBuilder)
        {
            hostBuilder.Services.AddTransient(typeof(IPublishInterceptor<>),typeof(PublishInterceptor<>));
            return hostBuilder;           
        }
    }
}