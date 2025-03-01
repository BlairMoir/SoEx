using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Test
{
    public abstract class TestEnvironmentBase
    {
        Type[] _servicesUnderTest = [];
        Type[] _policies = [];
        Action<ContainerBuilder> _autofacDependencies  = c => {};
        Action<IServiceCollection> _microsoftDependencies  = sc => {};

        public void SetupServices(params Type[] types)
        {
            _servicesUnderTest = types;
        }

        public void SetupContextPolicy(params Type[] policies)
        {
            _policies = policies;
        }

        public void DependencyContainerBuilder( Action<ContainerBuilder> cb)
        {
            _autofacDependencies = cb;
        }

        public void DependencyServiceCollection( Action<IServiceCollection> sc)
        {
            _microsoftDependencies = sc;
        }

        public Task TestService<T>(Func<T, Task> callerMock, params object[] mocks) where T : class
        {
            return MockServiceEnvironment<T>(GetServiceType<T>(), callerMock, mocks);
        }

        private Type GetServiceType<T>()
        {
            return _servicesUnderTest.Single(t => t.GetInterfaces().Contains(typeof(T)));
        }

        private void ServiceDependencies(ContainerBuilder containerBuilder)
        {
            _microsoftDependencies.Invoke(new ServiceCollectionBridge(containerBuilder));
            _autofacDependencies.Invoke(containerBuilder);
        }

        private async Task MockServiceEnvironment<T>(Type targetType, Func<T, Task> callerMock, params object[] mocks) where T : class
        {
            using (IDisposable environmentScope = TestContainer.CreateTestScope(_servicesUnderTest, _policies, ServiceDependencies))
            using (IDisposable testScope = TestContainer.CreateTestScope(mocks))
            {
                T poco = Container.Resolve<T>();
                await callerMock.Invoke(poco);
            }
        }
    }
}
