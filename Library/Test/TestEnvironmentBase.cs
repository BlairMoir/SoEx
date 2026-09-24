using Autofac;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Context;
using SoEx.Exceptions;
using SoEx.Hosting;
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

        public async Task TestService<S>(Func<S, Task> callerFunc, SoEx.Topology.System? system = null, KnownTypes? knownTypes = null) where S : notnull
        {
            await BuildAndInvoke(callerFunc, system, knownTypes);
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
            await BuildAndInvoke(callerFunc, newTopo, knownTypes);
        }

        public async Task BuildAndInvoke<S>(Func<S, Task> callerFunc, SoEx.Topology.System? system = null,
            KnownTypes? knownTypes = null) where S : notnull
        {
            string eventDirectory =
                Path.Combine(Path.GetTempPath(), "soex-test-events", Guid.NewGuid().ToString("N"));
            try
            {

                var topology = UniqueEventsDirectory(system ?? _topology, eventDirectory);
                using (var container = BuildContainer(topology, knownTypes))
                {
                    await Invoke(callerFunc, container);
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

        private async Task Invoke<S>(Func<S, Task> callerFunc, ILifetimeScope container) where S : notnull
        {
            var endpointRegister = container.Resolve<RegisteredEndpoints>();
            foreach (var endpoint in endpointRegister.Endpoints)
            {
                await endpoint.Listen();
            }

            try
            {
                using (var requestScope = container.BeginLifetimeScopeAsyncLocal())
                {
                    var proxy = requestScope.Resolve<S>();
                    await callerFunc.Invoke(proxy);
                }
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
