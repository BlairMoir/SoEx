namespace Example002.Host.Customer;

class Program
{
    static void Main(string[] args)
    {
        var hostBuilder = Example002.iFx.Hosting.Host.Dapr(5001, args);
        hostBuilder.Run();
    }
}
