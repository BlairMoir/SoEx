using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.NATS
{
    public static class NatsExtensions
    {
        public static IServiceCollection NatsClient(this IServiceCollection collection)
        {
            collection.AddTransient(typeof(NatsChannel<>), typeof(NatsChannel<>));
            collection.AddTransient(typeof(NatsEventChannel<>), typeof(NatsEventChannel<>));
            return collection;
        }
    }
}
