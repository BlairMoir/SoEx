using Example003.Common.Policy;
using Example003.iFx.Hosting.Test;
using Example003.iFx.Proxy;
using Example003.Manager.Membership.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoEx.Context;


namespace Example003.Host.InProc
{
    class Program
    {
        static void Main(string[] args)
        {
            var hostBuilder = Example003.iFx.Hosting.Host.InProc(args)
            .ConfigureServices(c =>
            {
                c.AddSingleton<IContextFlowPolicy, ContextFlowPolicy>();
            });
            hostBuilder.AddHostTest(HostTest);
            hostBuilder.Build().Run();
        }

        private static async Task HostTest()
        {
            List<Task> tasks = [];
            var proxy = Proxy.ForService<IMembershipManager>();
            tasks.Add(proxy.Profile());
            tasks.Add(Task.Delay(3000));
            Task.WaitAll([.. tasks]);
        }
    }
}
