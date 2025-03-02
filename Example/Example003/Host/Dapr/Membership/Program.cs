namespace Example003.Host.Membership;

class Program
{
    static void Main(string[] args)
    {        
        var hostBuilder = Example003.iFx.Hosting.Host.Dapr(5000, args);
        hostBuilder.Run();
    }
}
