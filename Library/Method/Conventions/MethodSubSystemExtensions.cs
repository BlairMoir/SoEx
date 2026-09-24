using Microsoft.Extensions.DependencyInjection;
using SoEx.Topology;
using SoEx.Transport.InProc;

namespace SoEx.Method.Conventions;

public static class MethodSubSystemExtensions
{
    public static MethodSubSystem AddEndpoints(this MethodSubSystem subSystem, Topology.Binding[] binding)
    {
        if (subSystem.EntryPoint == null)
        {
            throw new ArgumentNullException(nameof(subSystem.EntryPoint));
        }

        subSystem.EntryPoint = subSystem.EntryPoint with { Endpoints = [.. subSystem.EntryPoint.Endpoints, .. binding], };
        return subSystem;
    }

    public static MethodSubSystem AddInProcEndpoint<I>(this MethodSubSystem subSystem, IPipeline? customPipeline = null)
    {
        AddInProcEndpoint(subSystem, typeof(I),customPipeline);
        return subSystem;
    }

    public static MethodSubSystem AddInProcEndpoint(this MethodSubSystem subSystem, Type contract, IPipeline? customPipeline = null)
    {
        var genericType = typeof(InProcBinding<>).MakeGenericType(contract);
        var binding = (Binding)Activator.CreateInstance(genericType, subSystem.Name)!;
        if (customPipeline != null)
        {
            binding = binding with { Pipeline = customPipeline };
        }
        subSystem.AddEndpoints([binding]);
        return subSystem;
    }

    public static MethodSubSystem AddProxies(this MethodSubSystem subSystem, Topology.Client[] clients)
    {
        if (subSystem.EntryPoint == null)
        {
            throw new ArgumentNullException(nameof(subSystem.EntryPoint));
        }

        subSystem.EntryPoint = subSystem.EntryPoint with { Proxies =  [.. subSystem.EntryPoint.Proxies, .. clients] };
        return subSystem;
    }

    public static MethodSubSystem ConfigureServices(this MethodSubSystem subSystem, Action<IServiceCollection> services)
    {
        if (subSystem.EntryPoint == null)
        {
            throw new ArgumentNullException(nameof(subSystem.EntryPoint));
        }

        subSystem.EntryPoint = subSystem.EntryPoint with
        {
            ServiceCollection = subSystem.EntryPoint.ServiceCollection ?? new ServiceCollection()
        };
        services.Invoke(subSystem.EntryPoint.ServiceCollection!);
        return subSystem;
    }

    public static MethodComponent AddEngine(this MethodSubSystem subSystem, Type implementationType)
    {
        var host = new Topology.Host()
        {
            Implementation = implementationType,
            Endpoints = [],
            Proxies = []
        };
        MethodComponent methodComponent = new MethodComponent()
        {
            Host = host,
            SubSystem = subSystem.Name
        };
        subSystem.Engines = [.. subSystem.Engines, methodComponent];
        return methodComponent;
    }

    public static MethodComponent AddAccess(this MethodSubSystem subSystem, Type implementationType)
    {
        var host = new Topology.Host()
        {
            Implementation = implementationType,
            Endpoints = [],
            Proxies = []
        };
        MethodComponent methodComponent = new MethodComponent()
        {
            Host = host,
            SubSystem = subSystem.Name
        };
        subSystem.Access = [.. subSystem.Access, methodComponent];
        return methodComponent;
    }

    public static MethodComponent AddProxies(this MethodComponent component, Topology.Client[] clients)
    {
        if (component.Host == null)
        {
            throw new ArgumentNullException(nameof(component.Host));
        }

        component.Host = component.Host with { Proxies =  [.. component.Host.Proxies, .. clients]};
        return component;
    }
}
