using Autofac;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Context;
using SoEx.Exceptions;
using SoEx.Hosting;
using SoEx.Messaging.Chimera;
using SoEx.Topology;
using SoEx.Transport.Chimera;
using SoEx.Transport.InProc;

namespace SoEx.Test
{
    public abstract class TestEnvironmentBase
    {
        SoEx.Topology.System _topology = new Topology.System() { Clients = [], SubSystems = [] };

        Type[]? _policies;
        Type[]? _generics;
        KnownTypes _knownTypes = new KnownTypes();
        TestExceptionMode _testExceptionMode = new TestExceptionMode();

        public void DefaultConfiguration(SoEx.Topology.System topology)
        {
            _topology = topology;
        }

        public void SoExTestExceptionMode(ExceptionMode exceptionMode)
        {
            _testExceptionMode =  new TestExceptionMode(){ Mode = exceptionMode };
        }

        public void DefaultPolicies(Type[] policies)
        {
            _policies = policies;
        }

        public void DefaultKnownTypes(Type[] knownTypes)
        {
            _knownTypes = new KnownTypes(knownTypes);
        }

        public void GenericRegistrations(Type[] generics)
        {
            _generics = generics;
        }

        public async Task TestSystem(Func<SystemHarness, Task> systemFunc, SoEx.Topology.System? system = null,
            KnownTypes? knownTypes = null)
        {
            await BuildAndInvoke(systemFunc, system, knownTypes);
        }

        public async Task TestService<S>(Func<S, Task> callerFunc, SoEx.Topology.System? system = null, KnownTypes? knownTypes = null) where S : notnull
        {
            await BuildAndInvoke(system => system.Proxy(callerFunc), system, knownTypes);
        }

        public async Task TestComponent<I>(Func<I, Task> callerFunc, SoEx.Topology.System? system = null, KnownTypes? knownTypes = null) where I : class
        {
            var orginalTopo = system ?? _topology;
            var subsystemName = orginalTopo.SubSystems.First().Name;
            var newTopo = new SoEx.Topology.System()
            {
                SubSystems = orginalTopo.SubSystems,
                Clients = [new Client<I>() { Service = new InProcBinding<I>(subsystemName), SubSystem = subsystemName }],
                Defaults = orginalTopo.Defaults
            };
            await BuildAndInvoke(system => system.Proxy(callerFunc) , newTopo, knownTypes);
        }

        public async Task BuildAndInvoke(Func<SystemHarness, Task> systemFunc, SoEx.Topology.System? system = null,
            KnownTypes? knownTypes = null)
        {
            string eventDirectory =
                Path.Combine(Path.GetTempPath(), "soex-test-events", Guid.NewGuid().ToString("N"));
            try
            {
                var topology = UniqueEventsDirectory(system ?? _topology, eventDirectory);
                using (var container = BuildContainer(topology, knownTypes))
                {
                    await Invoke(systemFunc, container);
                }
            }
            finally
            {
                if (Directory.Exists(eventDirectory))
                {
                    Directory.Delete(eventDirectory, true);
                }
            }
        }

        private async Task Invoke(Func<SystemHarness, Task> systemFunc, ILifetimeScope container)
        {
            var endpointRegister = container.Resolve<RegisteredEndpoints>();
            var topics = container.Resolve<ChimeraTopic>();
            ChimeraOptions? options = null;
            foreach (var endpoint in endpointRegister.Endpoints)
            {
                if (endpoint is ChimeraEventEndpoint chimeraEventEndpoint
                    && chimeraEventEndpoint.ChimeraOptions is not null
                    && chimeraEventEndpoint.Topic is not null
                    )
                {
                    if (options is null)
                    {
                        options = chimeraEventEndpoint.ChimeraOptions;
                    }
                    topics.For(chimeraEventEndpoint.ChimeraOptions, chimeraEventEndpoint.Topic);
                }
                await endpoint.Listen();
            }

            try
            {
                await systemFunc.Invoke(new SystemHarness(container, options));
            }
            finally
            {
                foreach (var endpoint in endpointRegister.Endpoints)
                {
                    await endpoint.Close();
                }
            }
        }

        private ILifetimeScope BuildContainer(SoEx.Topology.System system, KnownTypes? knownTypes)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterSoEx(system, knownTypes ?? _knownTypes);
            builder.RegisterType<LoggerFactory>()
                            .As<ILoggerFactory>()
                            .SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();
            builder.RegisterGeneric(typeof(InProcChannel<>)).As(typeof(InProcChannel<>));
            builder.RegisterGeneric(typeof(ChimeraEventChannel<>)).As(typeof(ChimeraEventChannel<>));
            builder.RegisterType<InProcListeners>().SingleInstance().AsSelf();
            builder.RegisterType<ChimeraTopic>().SingleInstance();
            builder.RegisterInstance(_testExceptionMode).AsSelf();

            if (_policies is not null)
            {
                builder.RegisterTypes(_policies).As<IContextFlowPolicy>();
            }

            if (_generics is not null)
            {
                foreach (var type in _generics)
                {
                    builder.RegisterGeneric(type).As(type);
                }
            }

            var scope = builder.Build();
            return scope;
        }

        private Topology.System UniqueEventsDirectory(Topology.System topology, string eventDirectory)
        {
            Client RedirectClient(Client client) => client with { Service = CheckEventBinding(client.Service, eventDirectory) };
            Host RedirectHost(Host host) => host with
            {
                Endpoints = [..host.Endpoints.Select(s => CheckEventBinding(s, eventDirectory))],
                Proxies = [..host.Proxies.Select(RedirectClient)]
            };

            return topology with
            {
                SubSystems = [..topology.SubSystems.Select( s=> s with
                {
                    EntryPoint = RedirectHost(s.EntryPoint),
                    Components = [..s.Components.Select(RedirectHost)]
                })],
                Clients = [..topology.Clients.Select(RedirectClient)]
            };
        }

        private Binding CheckEventBinding(Binding binding, string eventDirectory)
        {
            if (binding is ChimeraEventBinding chimeraEventBinding)
            {
                return chimeraEventBinding with
                {
                    Options = chimeraEventBinding.Options with { RootDirectory = eventDirectory }
                };
            }
            return binding;
        }
    }
}
