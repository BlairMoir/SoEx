using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.Azure.ServiceBus
{
    public static class TopicExtensions
    {
        public static IServiceCollection ASBTopicClient(this IServiceCollection collection)
        {
            collection.AddTransient(typeof(TopicEventChannel<>), typeof(TopicEventChannel<>));
            return collection;
        }
    }
}
