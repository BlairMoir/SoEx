using SoEx.Topology;

namespace SoEx.Method.Conventions;

public class SystemBuilder
{
    private Dictionary<string, MethodSubSystem> _methodSubSystems = [];
    private HashSet<string> _utilitySubsystems = [];
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
        _utilitySubsystems.Add(subSystem.Name);
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
        _events.AddRange(eventTypes.Except(_events));
        return this;
    }

    public Topology.System Build(IPipeline? defaultPipeline = null)
    {
        var managers = _methodSubSystems.Values.Where( m => !IsUtility(m)).ToArray();
        var subscriptions = Subscriptions(managers);
        RefuseUnboundEvents(subscriptions);

        var eventBindings = EventBindings(managers);
        Client[] eventClients = [.. eventBindings.Values.Select( c=> c.ToClient())];
        Client[] sharedUtilities = SharedUtilityClients();

        List<Topology.SubSystem> subSystems = new List<Topology.SubSystem>();

        foreach (var subsystem in _methodSubSystems.Values)
        {
            if (IsUtility(subsystem))
            {
                subSystems.Add(ComposeUtility(subsystem));
            }
            else
            {
                subSystems.Add(ComposeManager(subsystem, subscriptions[subsystem.Name], eventBindings, eventClients, sharedUtilities ));
            }
        }

        var system = new Topology.System()
        {
            Clients = [],
            SubSystems = [ .. subSystems,],
            Defaults = defaultPipeline,
        };

        return system;
    }

    private bool IsUtility(MethodSubSystem subsystem)
    {
        return _utilitySubsystems.Contains(subsystem.Name);
    }

    private static IEnumerable<Type> ImplementedEvents(MethodSubSystem subsystem)
    {
        return subsystem.EntryPoint!.Implementation.GetInterfaces().Where( w=> w.Name.EndsWith(Keywords.Event));
    }

    private static Dictionary<string, Type[]> Subscriptions(MethodSubSystem[] managers)
    {
        return managers.ToDictionary(s => s.Name, s => ImplementedEvents(s)
            .Where( ev=> !s.EntryPoint!.Endpoints.Any( a=> a.Contract ==ev))
            .ToArray()
        );
    }

    private void RefuseUnboundEvents(Dictionary<string, Type[]> subscriptions)
    {
        if (_eventBinding is not null)
            return;
        var unboundEvents = subscriptions.Values.SelectMany(e=> e).Union(_events).ToArray();
        if (unboundEvents.Any())
        {
            var message = $"Events {string.Join(", ", unboundEvents.Select(e => e.Name))} have no binding";
            var fix = $"Define binding using {nameof(WithEvents)} before calling {nameof(Build)}";
            throw new InvalidOperationException($"{message} {fix}");
        }
    }

    private Dictionary<Type, Binding> EventBindings(MethodSubSystem[] managers)
    {
        if (_eventBinding is null)
            return [];

        return _events.Union(managers.SelectMany(ImplementedEvents)).ToDictionary(e => e, e => _eventBinding(e));
    }

    private Client[] SharedUtilityClients()
    {
        return [.. _methodSubSystems.Values.Where(IsUtility).SelectMany(s => ClientsFor(s.EntryPoint!, s.Name))];
    }

    private static Client[] ClientsFor(IEnumerable<MethodComponent> components, string subSystemName)
    {
        return [.. components.SelectMany(c=> ClientsFor(c.Host, subSystemName))];
    }

    private static IEnumerable<Client> ClientsFor(Topology.Host host, string subSystemName)
    {
        return host.Endpoints.Select(s => s.ToClient(subSystemName));
    }

    private static Topology.Host EntryPoint(Topology.Host entryPoint, Binding[] subscriptions, Client[] components,
        Client[] eventclients)
    {
        return entryPoint with
        {
            Endpoints = [..entryPoint.Endpoints, .. subscriptions],
            Proxies =
            [
                ..entryPoint.Proxies, .. components,
                ..eventclients.Where(w => !entryPoint.Proxies.Any(p => p.Service.Contract == w.Service.Contract))
            ]
        };

    }

    private static Topology.SubSystem ComposeUtility(MethodSubSystem subsystem)
    {
        var utilitySubsystem = Compose(subsystem, [],[],[]);
        return utilitySubsystem;
    }

    private static Topology.SubSystem ComposeManager(MethodSubSystem subSystem, Type[] subscribedEvents,
        Dictionary<Type, Binding> eventBindings, Client[] eventClients, Client[] sharedUtilities)
    {
        Binding[] subscriptions = [.. subscribedEvents.Select(e => eventBindings[e])];
        var managerSubsystem = Compose(subSystem, subscriptions, eventClients, sharedUtilities);
        return managerSubsystem;
    }

    private static SubSystem Compose(MethodSubSystem methodSubsystem, Binding[] subscriptions, Client[] eventClients,
        Client[] sharedUtilities)
    {
        if(methodSubsystem.EntryPoint is null)
            throw new ArgumentNullException($"{nameof(methodSubsystem.Name)} requires an entry point");

        Client[] access = ClientsFor(methodSubsystem.Access, methodSubsystem.Name);
        Client[] engines = ClientsFor(methodSubsystem.Engines, methodSubsystem.Name);
        Client[] utilities = [..ClientsFor(methodSubsystem.Utilities, methodSubsystem.Name),..sharedUtilities];

        var topologySubsystem = new SubSystem()
        {
            Name = methodSubsystem.Name,
            EntryPoint = EntryPoint(methodSubsystem.EntryPoint!, subscriptions, [ ..engines, .. access, .. utilities], eventClients),
            Components = [
                .. methodSubsystem.Engines.Select(e=> WithProxies(e.Host, [.. access, .. utilities])),
                .. methodSubsystem.Access.Select(e=> WithProxies(e.Host, [.. utilities])),
                .. methodSubsystem.Utilities.Select( u=> u.Host)
            ]
        };

        return topologySubsystem;
    }

    private static Topology.Host WithProxies(Topology.Host host, Client[] clients)
    {
        return host with { Proxies = [..host.Proxies, ..clients] };
    }
}
