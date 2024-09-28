namespace Example002.Host.Membership;

class Program
{
    static void Main(string[] args)
    {        
        var hostBuilder = Example002.iFx.Hosting.Host.Dapr(5000, args);
        hostBuilder.Run();
    }
}
