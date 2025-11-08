using Microsoft.Extensions.Hosting;

namespace SoExTemplate.iFx.Hosting.Test
{
    public class TestService : BackgroundService
    {
        private readonly IHostApplicationLifetime m_AppLifetime;
        private readonly Func<Task> m_TestAction;

        public TestService(
            IHostApplicationLifetime appLifetime,
            Func<Task> testAction)
        {
            m_AppLifetime = appLifetime;
            m_TestAction = testAction;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000);
            await m_TestAction.Invoke().ConfigureAwait(false);
            m_AppLifetime.StopApplication();
        }
    }
}
