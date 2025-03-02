namespace Example003.Host.Customer;

class Program
{
    static void Main(string[] args)
    {
        var hostBuilder = Example003.iFx.Hosting.Host.Dapr(5001, args);
        hostBuilder.Run();
    }
}
