using Example002.Common.Policy;
using Example002.iFx.Hosting.Test;
using Example002.iFx.Proxy;
using Example002.Manager.Membership.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoEx.Context;


namespace Example002.Host.InProc
{
    class Program
    {
        static void Main(string[] args)
        {
            var hostBuilder = Example002.iFx.Hosting.Host.InProc(args)
            .ConfigureServices(c =>
            {
                c.AddSingleton<IContextFlowPolicy, ContextFlowPolicy>();
            });
            hostBuilder.AddHostTest(HostTest);
            hostBuilder.Build().Run();
        }

        private static async Task HostTest()
        {
            var proxy = Proxy.ForService<IMembershipManager>();
            await proxy.Profile();
        }
    }
}
