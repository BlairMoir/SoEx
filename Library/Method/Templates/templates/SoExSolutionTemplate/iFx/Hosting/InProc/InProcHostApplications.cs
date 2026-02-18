using Autofac;
using SoExTemplate.iFx.Hosting.Test;
using Microsoft.Extensions.Hosting;

namespace SoExTemplate.iFx.Hosting;

public class InProcHostApplications
{
    public required HostApplicationBuilder ServiceHost { get; init; }
    public required HostApplicationBuilder ClientHost { get; init; }

    public InProcHostApplications WithTestClient(Func<ILifetimeScope, Task> testAction, bool shutdownWhenCompleted = false)
    {
        ClientHost.AddTestClient(testAction, shutdownWhenCompleted);
        return this;
    }

    public async Task RunAsync()
    {
        var serviceHostCancellationTokenSource = new CancellationTokenSource();
        var serviceTask = ServiceHost.Build().RunAsync(serviceHostCancellationTokenSource.Token);
        await ClientHost.Build().RunAsync();
        await serviceHostCancellationTokenSource.CancelAsync();
        await serviceTask;
    }
}
