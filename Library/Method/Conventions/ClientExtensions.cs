using System.Reflection;
using SoEx.Topology;

namespace SoEx.Method.Conventions;

public static class ClientExtensions
{
    private static readonly MethodInfo s_create =
        typeof(ClientExtensions).GetMethod(nameof(Create), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static Client ToClient(this Binding service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return service.ToClient(service.SubSystem);
    }

    public static Client ToClient(this Binding service, string subSystem)
    {
        ArgumentNullException.ThrowIfNull(service);
        return (Client)s_create.MakeGenericMethod(service.Contract).Invoke(null, [service,subSystem])!;
    }

    private static Client Create<I>(Binding service, string subsystem) where I : class
    {
        return new Client<I>() {Service = service, SubSystem =  subsystem};
    }
}
