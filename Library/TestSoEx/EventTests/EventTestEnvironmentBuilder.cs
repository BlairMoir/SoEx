using Microsoft.Extensions.DependencyInjection;
using SoEx.Transport.InProc;
using SoEx.Transport.SBQueue;
using SoEx.Transport.SQS;
using SoEx.Transport.ThreadChannel;
using SoEx.Topology;

namespace SoEx.TestSoEx.EventTests;

public class EventTestEnvironmentBuilder
{
    private const string senderSubsystemName = "sender";
    private const string receiverSubsystemName = "receiver";
    private IEventMonitor? _eventMonitor;
    private ManagerDescriptor? _senderParent;
    private readonly List<ManagerDescriptor> _senders = [];
    private readonly List<ManagerDescriptor> _receivers = [];
    private readonly List<Binding> _events = [];

    private record ManagerDescriptor(Type Implementation, Binding Binding) { }

    public EventTestEnvironmentBuilder WithEventMonitor(IEventMonitor eventMonitor)
    {
        _eventMonitor = eventMonitor;
        return this;
    }

    public EventTestEnvironmentBuilder WithSender<TContract, TService>()
    {
        _senders.Add(new ManagerDescriptor(typeof(TService), new InProcBinding<TContract>(senderSubsystemName)));
        return this;
    }

    public EventTestEnvironmentBuilder WithReceiver<TContract, TService>()
    {
        _receivers.Add(new ManagerDescriptor(typeof(TService), new InProcBinding<TContract>(receiverSubsystemName)));
        return this;
    }

    public EventTestEnvironmentBuilder WithServiceBusEvent<T>(SBConfig config)
    {
        _events.Add(new SBQueueBinding<T>(config));
        return this;
    }

    public EventTestEnvironmentBuilder WithSQSEvent<T>(SQSConfig config)
    {
        _events.Add(new SQSBinding<T>(config));
        return this;
    }

    public EventTestEnvironmentBuilder WithChannelEvent<T>()
    {
        _events.Add(new UnsafeThreadChannelBinding<T>());
        return this;
    }

    public EventTestEnvironmentBuilder WithSenderParent<TContract, TService>()
    {
        _senderParent = new ManagerDescriptor(typeof(TService), new InProcBinding<TContract>(senderSubsystemName));
        return this;
    }

    public EventTestEnvironment Build()
    {
        EventTestEnvironment environment = new();
        IServiceCollection services = new ServiceCollection();
        if (_eventMonitor != null)
        {
            services = services.AddSingleton(_ => _eventMonitor!);
        }
        List<Client> clients = [];
        List<SubSystem> subsystems = [];
        foreach (Binding eventBinding in _events)
        {
            Client client = BuildClient(eventBinding.Contract, "events", eventBinding);
            clients.Add(client);
        }

        // generate sender subsystem
        if (_senderParent == null)
        {
            foreach (ManagerDescriptor sender in _senders)
            {
                subsystems.Add(new SubSystem()
                {
                    Name = senderSubsystemName,
                    EntryPoint = new Host()
                    {
                        Proxies = [],
                        Implementation = sender.Implementation,
                        Endpoints = [sender.Binding],
                    },
                    Components = [],
                });
                Client client = BuildClient(sender.Binding.Contract, senderSubsystemName, sender.Binding);
                clients.Add(client);
            }
        }
        else
        {
            List<Host> components = new List<Host>();
            List<Client> subsystemClients = new List<Client>();
            foreach (ManagerDescriptor sender in _senders)
            {
                components.Add(new Host()
                {
                    Proxies = [],
                    Implementation = sender.Implementation,
                    Endpoints = [sender.Binding],
                });
                Client subsystemClient = BuildClient(sender.Binding.Contract, senderSubsystemName, sender.Binding);
                subsystemClients.Add(subsystemClient);
            }
            subsystems.Add(new SubSystem()
            {
                Name = senderSubsystemName,
                EntryPoint = new Host()
                {
                    Proxies = [.. subsystemClients],
                    Implementation = _senderParent.Implementation,
                    Endpoints = [_senderParent.Binding],
                },
                Components = [.. components]
            });
            Client client = BuildClient(_senderParent.Binding.Contract, senderSubsystemName, _senderParent.Binding);
            clients.Add(client);
        }

        // generate receiver subsystem
        foreach (ManagerDescriptor receiver in _receivers)
        {
            List<Binding> receiverBindings = [receiver.Binding];
            foreach (Binding eventBinding in _events)
            {
                receiverBindings.Add(eventBinding);
            }
            subsystems.Add(new SubSystem()
            {
                Name = receiverSubsystemName,
                EntryPoint = new Host()
                {
                    Proxies = [],
                    Implementation = receiver.Implementation,
                    Endpoints = [.. receiverBindings],
                    ServiceCollection = services,
                },
                Components = [],
            });
            Client client = BuildClient(receiver.Binding.Contract, receiverSubsystemName, receiver.Binding);
            clients.Add(client);
        }

        // generate topology from subsystems and clients
        Topology.System topology = new Topology.System()
        {
            SubSystems = [.. subsystems],
            Clients = [.. clients],
        };
        environment.DefaultConfiguration(topology);
        return environment;
    }

    private static Client BuildClient(Type bindingContract, string subsystemName, Binding binding)
    {
        Type clientType = typeof(Client<>).MakeGenericType(bindingContract);
        Client client = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(clientType) as Client
            ?? throw new InvalidOperationException("Failed to create client instance.");

        typeof(Client).GetProperty(nameof(Client.SubSystem))!.SetValue(client, subsystemName);
        typeof(Client).GetProperty(nameof(Client.Service))!.SetValue(client, binding);
        return client;
    }
}
