using System.Xml;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoEx.Topology;

namespace SoEx.Hosting;

public class EndpointLifetimeService : IHostedLifecycleService
{
    readonly ILogger<EndpointLifetimeService> _logger;
    readonly RegisteredEndpoints _registeredEndpoints;

    public EndpointLifetimeService(ILogger<EndpointLifetimeService> logger, RegisteredEndpoints registeredEndpoints)
    {
        _logger = logger;
        _registeredEndpoints = registeredEndpoints;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        List<Task> startupTasks = new List<Task>();
        _logger.LogDebug("The host has started, attempting to start listeners");
        foreach (var endpoint in _registeredEndpoints.Endpoints)
        {
            startupTasks.Add(Listen(endpoint));
        }
        await Task.WhenAll(startupTasks);
        _logger.LogDebug("Listeners started");
    }

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        List<Task> shutdownTasks = new List<Task>();
        _logger.LogDebug("The host has requested shutdown, attempting to close listeners");
        foreach (var endpoint in _registeredEndpoints.Endpoints)
        {
            try
            {
                shutdownTasks.Add(Close(endpoint));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Endpoint listen error: {ErrorMessage}", ex.Message);
            }
        }

        await Task.WhenAll(shutdownTasks);
        _logger.LogDebug("Listeners closed");
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task Listen(IEndpoint endpoint)
    {
        try
        {
            await endpoint.Listen();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Endpoint {Endpoint} did not start listening", endpoint.GetType());
        }
    }

    private async Task Close(IEndpoint endpoint)
    {
        try
        {
            await endpoint.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Endpoint {Endpoint} did not close cleanly", endpoint.GetType());
        }
    }
}
