using Example002.iFx.Proxy;
using Example002.Manager.Membership.Interface;

namespace Example002.Host.TestClient;

class Program
{
    static void Main(string[] args)
    {
        var hostBuilder = Example002.iFx.Hosting.Host.Dapr(5003, args);
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
