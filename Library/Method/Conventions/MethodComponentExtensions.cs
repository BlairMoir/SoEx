using Microsoft.Extensions.DependencyInjection;
using SoEx.Topology;
using SoEx.Transport.InProc;

namespace SoEx.Method.Conventions;

public static class MethodComponentExtensions
{
    public static MethodComponent AddEndpoints(this MethodComponent component, Topology.Binding[] binding)
    {
        if (component.Host == null)
        {
            throw new ArgumentNullException(nameof(component.Host));
        }

        component.Host = component.Host with { Endpoints = [.. component.Host.Endpoints, .. binding] };
        return component;
    }

    public static MethodComponent AddInProcEndpoint<I>(this MethodComponent component)
    {
        component.AddEndpoints([new InProcBinding<I>(component.SubSystem)]);
        return component;
    }

    public static MethodComponent AddInProcEndpoint(this MethodComponent component, Type contract)
    {
        var genericType = typeof(InProcBinding<>).MakeGenericType(contract);
        var binding = (Binding)Activator.CreateInstance(genericType, component.SubSystem)!;
        component.AddEndpoints([binding]);
        return component;
    }

    public static MethodComponent ConfigureServices(this MethodComponent component, Action<IServiceCollection> services)
    {
        if (component.Host == null)
        {
            throw new ArgumentNullException(nameof(component.Host));
        }

        component.Host = component.Host with
        {
            ServiceCollection = component.Host.ServiceCollection ?? new ServiceCollection()
        };
        services.Invoke(component.Host.ServiceCollection);
        return component;
    }
}
