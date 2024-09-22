namespace SoEx.Test
{
    public abstract class TestEnvironmentBase
    {
        Type[] _servicesUnderTest = [];
        Type[] _policies = [];

        public void SetupServices(params Type[] types)
        {
            _servicesUnderTest = types;
        }

        public void SetupContextPolicy(params Type[] policies)
        {
            _policies = policies;
        }

        public Task TestService<T>(Func<T, Task> callerMock, params object[] mocks) where T : class
        {
            return MockServiceEnvironment<T>(GetServiceType<T>(), callerMock, mocks);
        }

        private Type GetServiceType<T>()
        {
            return _servicesUnderTest.Single(t => t.GetInterfaces().Contains(typeof(T)));
        }

        private async Task MockServiceEnvironment<T>(Type targetType, Func<T, Task> callerMock, params object[] mocks) where T : class
        {
            using (IDisposable environmentScope = TestContainer.CreateTestScope(_servicesUnderTest, _policies))
            using (IDisposable testScope = TestContainer.CreateTestScope(mocks))
            {
                T poco = Container.Resolve<T>();
                await callerMock.Invoke(poco);
            }
        }
    }
}
