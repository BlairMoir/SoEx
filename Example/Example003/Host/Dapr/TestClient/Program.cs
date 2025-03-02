using Example003.iFx.Proxy;
using Example003.Manager.Membership.Interface;

namespace Example003.Host.TestClient;

class Program
{
    static void Main(string[] args)
    {
        var hostBuilder = Example003.iFx.Hosting.Host.Dapr(5003, args);
        var daprHost = hostBuilder.RunAsync();

        var clientTest = Task.Run(ClientTest);
        Task.WaitAll(daprHost,clientTest);
    }

    private static async Task ClientTest()
    {
#if DEBUG 
        System.Diagnostics.Debugger.Break();
#endif     
        try{
            var proxy = Proxy.ForService<IMembershipManager>();
            await proxy.Profile();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
