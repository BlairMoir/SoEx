using Autofac;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SoExTemplate.iFx.Hosting.Test
{
    public static class HostTestExtensions
    {
        public static IHostApplicationBuilder AddTestClient(
            this IHostApplicationBuilder host,
            Func<ILifetimeScope, Task> testAction,
            bool shutdownWhenCompleted = false
        )
        {
            if (testAction is not null)
            {
                host.Services.AddHostedService(serviceProvider =>
                {
                    IHostApplicationLifetime? hostApplicationLifetime = serviceProvider.GetService<IHostApplicationLifetime>();
                    ILifetimeScope? rootScope = serviceProvider.GetService<ILifetimeScope>();
                    Debug.Assert(hostApplicationLifetime is not null);
                    Debug.Assert(rootScope is not null);

                    return new TestClient(hostApplicationLifetime, rootScope, testAction, shutdownWhenCompleted);
                });
            }
            return host;
        }
    }
}
