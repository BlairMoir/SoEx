using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.InProc
{
    public static class InProcExtensions
    {
        public static IServiceCollection InProcClient(this IServiceCollection collection)
        {
            SharedInProcListeners([collection]);
            return collection;
        }

        public static IServiceCollection InProcClientWithSpan(this IServiceCollection collection, params IServiceCollection[] otherCollections)
        {
            SharedInProcListeners([collection, .. otherCollections]);
            return collection;
        }

        public static void InProcClientWithSpan(params IServiceCollection[] otherCollections)
        {
            SharedInProcListeners([.. otherCollections]);
        }

        private static void SharedInProcListeners(IServiceCollection[] serviceCollections)
        {
            var inProcListeners = new InProcListeners();
            foreach (var sc in serviceCollections)
            {
                sc.AddTransient(typeof(InProcChannel<>), typeof(InProcChannel<>));
                sc.AddSingleton(typeof(InProcListeners), inProcListeners);
            }
        }

        public static void InProcHost() { }
    }
}
