using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SoExTemplate.iFx.Hosting.Test
{
    public class TestClient : BackgroundService
    {
        readonly IHostApplicationLifetime _hostApplicationLifetime;
        readonly Autofac.ILifetimeScope _lifetimeScope;
        readonly Func<Autofac.ILifetimeScope, Task> _testAction;
        readonly bool _shutdownWhenCompleted;
        public TestClient(IHostApplicationLifetime hostApplicationLifetime, Autofac.ILifetimeScope lifetimeScope, Func<Autofac.ILifetimeScope, Task> testAction, bool shutdownWhenCompleted)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _lifetimeScope = lifetimeScope;
            _testAction = testAction;
            _shutdownWhenCompleted = shutdownWhenCompleted;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _testAction.Invoke(_lifetimeScope).ConfigureAwait(false);
            if (_shutdownWhenCompleted)
            {
                _hostApplicationLifetime.StopApplication();
            }
        }
    }
}
