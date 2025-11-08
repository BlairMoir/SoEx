using Autofac;
using SoEx;


namespace SoExTemplate.Host.InProc
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hosts = SoExTemplate.iFx.Hosting.Host.InProc(args)
                .WithTestClient(TestClient, shutdownWhenCompleted: true);
            await hosts.RunAsync();
        }

        private static async Task TestClient(ILifetimeScope lifetimeScope)
        {
            using (var requestScope = lifetimeScope.BeginLifetimeScopeAsyncLocal())
            {
                // var proxy = Proxy.ForService<I_XXX_Manager>();
                // await proxy._Operation_();
                throw new NotImplementedException();
            }
            await Task.Delay(1000);
        }
    }
}
