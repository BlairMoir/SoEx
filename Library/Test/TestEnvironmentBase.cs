using Autofac;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Context;
using SoEx.Hosting;
using SoEx.Topology;
using SoEx.Transport.InProc;
using SoEx.Transport.ThreadChannel;

namespace SoEx.Test
{
    public abstract class TestEnvironmentBase
    {
        SoEx.Topology.System _topology = new Topology.System() { Clients = [], SubSystems = [] };

        Type[]? _policies;
        Type[]? _generics;

        public void DefaultConfiguration(SoEx.Topology.System topology)
        {
            _topology = topology;
        }

        public void DefaultPolicies(Type[] policies)
        {
            _policies = policies;
        }

        public void GenericRegistrations(Type[] generics)
        {
            _generics = generics;
        }

        public async Task TestService<S>(Func<S, Task> callerFunc, SoEx.Topology.System? system = null) where S : notnull
        {
            var container = BuildContainer(system);
            using (var requestScope = container.BeginLifetimeScopeAsyncLocal())
            {
                var proxy = requestScope.Resolve<S>();
                await callerFunc.Invoke(proxy);
            }
        }

        public async Task TestComponent<I>(Func<I, Task> callerFunc, SoEx.Topology.System? system = null) where I : class
        {
            var orginalTopo = system ?? _topology;
            var subsystemName = orginalTopo.SubSystems.First().Name;
            var newTopo = new SoEx.Topology.System()
            {
                SubSystems = orginalTopo.SubSystems,
                Clients = [new Client<I>() { Service = new InProcBinding<I>(subsystemName), SubSystem = subsystemName }],
                Defaults = orginalTopo.Defaults
            };
            var container = BuildContainer(newTopo);
            using (var requestScope = container.BeginLifetimeScopeAsyncLocal())
            {
                var proxy = requestScope.Resolve<I>();
                await callerFunc.Invoke(proxy);
            }
        }

        private ILifetimeScope BuildContainer(SoEx.Topology.System? system)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterSoEx(system ?? _topology);
            builder.RegisterType<LoggerFactory>()
                            .As<ILoggerFactory>()
                            .SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();
            builder.RegisterGeneric(typeof(InProcChannel<>)).As(typeof(InProcChannel<>));
            builder.RegisterGeneric(typeof(UnsafeThreadChannelChannel<>)).As(typeof(UnsafeThreadChannelChannel<>));
            builder.RegisterGeneric(typeof(UnsafeThreadEventChannel<>)).As(typeof(UnsafeThreadEventChannel<>)).SingleInstance();
            builder.RegisterType<InProcListeners>().SingleInstance().AsSelf();

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
            var endpointRegister = scope.Resolve<RegisteredEndpoints>();
            foreach (var endpoint in endpointRegister.Endpoints)
            {
                endpoint.Listen();
            }

            return scope;
        }
    }
}
