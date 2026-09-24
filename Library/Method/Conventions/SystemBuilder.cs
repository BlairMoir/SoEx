using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SoEx.Topology;
using SoEx.Transport.InProc;

namespace SoEx.Method.Conventions;

public class MethodSubSystem
{
    public required string Name { get; init; }
    public Topology.Host? EntryPoint { get; set; }
    public MethodComponent[] Engines { get; set; } = [];
    public MethodComponent[] Access { get; set; } = [];
}

public class MethodComponent
{
    public required string SubSystem { get; init; }
    public required Topology.Host Host { get; set; }
}

public static class MethodSubSystemExtensions
{
    public static MethodSubSystem AddEndpoints(this MethodSubSystem subSystem, Topology.Binding[] binding)
    {
        if (subSystem.EntryPoint == null)
        {
            throw new ArgumentNullException(nameof(subSystem.EntryPoint));
        }

        var entryPoint = subSystem.EntryPoint;
        subSystem.EntryPoint = new Topology.Host()
        {
            Implementation = entryPoint.Implementation,
            Endpoints = [.. entryPoint.Endpoints, .. binding],
            Proxies = entryPoint.Proxies,
            ServiceCollection = entryPoint.ServiceCollection
        };

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
        return component;
    }
}


public class SystemBuilder
{
    private Dictionary<string, MethodSubSystem> _methodSubSystems = [];

    public MethodSubSystem AddManager(Type implementationType, string? subSystemName = null)
    {
        const string ManagerComponentString = "Manager";

        var typeName = implementationType.Name;

        if (!typeName.EndsWith(ManagerComponentString))
        {
            throw new ArgumentException("Entry point of a subsystem must be a manager");
        }

        var componenTypeIndex = typeName.IndexOf(ManagerComponentString);
        var defaultSubSystemName = typeName.Substring(0, componenTypeIndex);
        var subSystem = AddSubSystem(subSystemName ?? defaultSubSystemName);
        subSystem.EntryPoint = new Topology.Host() { Implementation = implementationType, Endpoints = [], Proxies = [] };
        return subSystem;
    }

    public MethodSubSystem AddSubSystem(string name)
    {
        var subSystem = new MethodSubSystem()
        {
            Name = name
        };
        _methodSubSystems.Add(subSystem.Name, subSystem);
        return subSystem;
    }

    public Topology.System Build(IPipeline? defaultPipeline = null)
    {
        List<Topology.SubSystem> subSystems = new List<Topology.SubSystem>();
        foreach (var subsystemName in _methodSubSystems.Keys)
        {
            ComponentProxies(subsystemName);
            var subsystem = _methodSubSystems[subsystemName];
            subSystems.Add(new Topology.SubSystem()
            {
                Name = subsystem.Name,
                EntryPoint = subsystem.EntryPoint!,
                Components = [.. subsystem.Engines.Select(s => s.Host), .. subsystem.Access.Select(s => s.Host)]
            });
        }
        return new Topology.System() { Clients = [], SubSystems = [..subSystems], Defaults = defaultPipeline };
    }

    private void ComponentProxies(string subsystemName)
    {
        var subsystem = _methodSubSystems[subsystemName];
        foreach (var access in subsystem.Access)
        {
            var accessClients = access.Host.Endpoints.Select(s => ToClient(s, subsystemName)).ToArray();
            subsystem.AddProxies(accessClients);
            foreach (var engine in subsystem.Engines)
            {
                engine.Host =  engine.Host with { Proxies = [.. engine.Host.Proxies, .. accessClients] };
            }
        }
        foreach (var engine in subsystem.Engines)
        {
            var engineClients = engine.Host.Endpoints.Select(s => ToClient(s, subsystemName)).ToArray();
            subsystem.AddProxies(engineClients);
        }
    }

    private static readonly MethodInfo s_newClient = typeof(SystemBuilder).GetMethod(nameof(NewClient),
        BindingFlags.NonPublic | BindingFlags.Static)!;
    private static Client NewClient<I>(Binding service, string subsystem) where I : class
    {
        return new Client<I>() {Service = service, SubSystem =  subsystem};
    }
    private static Client ToClient(Binding binding, string subsystemName)
    {
        var client = s_newClient.MakeGenericMethod(binding.Contract).Invoke(null, [ binding, subsystemName ])!;
        return (Client)client;
    }
}
