using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.ASBTopic
{
    public static class ASBTopicExtensions
    {
        public static IServiceCollection ASBTopicClient(this IServiceCollection collection)
        {
            collection.AddTransient(typeof(ASBTopicEventChannel<>), typeof(ASBTopicEventChannel<>));
            return collection;
        }
    }
}
