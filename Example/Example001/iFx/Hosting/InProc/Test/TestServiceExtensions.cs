using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Example001.iFx.Hosting.Test
{
    public static class TestServiceExtensions
    {
        public static IHostBuilder AddHostTest(
            this IHostBuilder host,
            Func<Task> testAction)
        {
            return host.ConfigureServices((hostContext, services) =>
            {
                if (testAction is not null)
                {
                    services.AddHostedService(serviceProvider =>
                    {
                        IHostApplicationLifetime? hostApplicationLifetime = serviceProvider.GetService<IHostApplicationLifetime>();
                        Debug.Assert(hostApplicationLifetime is not null);

                        return new TestService(hostApplicationLifetime, testAction);
                    });
                }
            });
        }

    }
}
