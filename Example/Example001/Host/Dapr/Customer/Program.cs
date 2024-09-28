namespace Example001.Host.Customer;

class Program
{
    static void Main(string[] args)
    {
        var hostBuilder = Example001.iFx.Hosting.Host.Dapr(5001, args);
        hostBuilder.Run();
    }
}
