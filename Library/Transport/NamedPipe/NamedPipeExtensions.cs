using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.NamedPipe
{
    public static class NamedPipeExtensions
    {
        public static IServiceCollection NamedPipedClient(this IServiceCollection collection)
        {
            collection.AddTransient(typeof(NamedPipeChannel<>), typeof(NamedPipeChannel<>));
            collection.AddTransient(typeof(NamedPipeEventChannel<>), typeof(NamedPipeEventChannel<>));
            return collection;
        }
    }
}
