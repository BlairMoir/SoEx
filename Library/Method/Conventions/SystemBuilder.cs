using System.Reflection;
using SoEx.Topology;

namespace SoEx.Method.Conventions;

public class SystemBuilder
{
    private Dictionary<string, MethodSubSystem> _methodSubSystems = [];
    private HashSet<string> _utiltySubsystems = [];
    private Func<Type, Binding>? _eventBinding;
    private List<Type> _events = [];

    private static string ComponentName(Type implementationType, string suffix, string error)
    {
        var typeName = implementationType.Name;
        if(!typeName.EndsWith(suffix))
            throw new ArgumentException(error);
        return typeName[..^suffix.Length];
    }

    public MethodSubSystem AddManager(Type implementationType, string? subSystemName = null)
    {
        var defaultSubSystemName = ComponentName(implementationType, Keywords.Manager,
            "Entry point of a subsystem must be a manager");
        var subSystem = AddSubSystem(subSystemName ?? defaultSubSystemName);
        subSystem.EntryPoint = new Topology.Host() { Implementation = implementationType, Endpoints = [], Proxies = [] };
        return subSystem;
    }

    public MethodSubSystem AddUtility(Type implementationType, string? subSystemName = null)
    {
        var defaultSubSystemName = ComponentName(implementationType, Keywords.Utility,
            "A utility name must end in Utility");
        var subSystem = AddSubSystem(subSystemName ?? defaultSubSystemName);
        subSystem.EntryPoint = new Topology.Host() { Implementation = implementationType, Endpoints = [], Proxies = [] };
        _utiltySubsystems.Add(subSystem.Name);
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

    public SystemBuilder WithEvents(Func<Type, Binding> eventBinding)
    {
        _eventBinding = eventBinding;
        return this;
    }

    public SystemBuilder AddEvents(params Type[] eventTypes)
    {
        _events.AddRange(_events.Except(_events));
        return this;
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
            var accessClients = access.Host.Endpoints.Select(s => s.ToClient()).ToArray();
            subsystem.AddProxies(accessClients);
            foreach (var engine in subsystem.Engines)
            {
                engine.Host =  engine.Host with { Proxies = [.. engine.Host.Proxies, .. accessClients] };
            }
        }
        foreach (var engine in subsystem.Engines)
        {
            var engineClients = engine.Host.Endpoints.Select(s => s.ToClient()).ToArray();
            subsystem.AddProxies(engineClients);
        }
    }
}
