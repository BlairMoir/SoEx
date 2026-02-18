// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            try
            {
                startupTasks.Add(endpoint.Listen());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Endpoint listen error: {ErrorMessage}", ex.Message);
            }
        }
        await Task.WhenAll(startupTasks).ContinueWith(_ => _logger.LogDebug("Listeners started"));
    }

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        List<Task> shutdownTasks = new List<Task>();
        _logger.LogDebug("The host has requested shutdown, attempting to close listeners");
        foreach (var endpoint in _registeredEndpoints.Endpoints)
        {
            try
            {
                shutdownTasks.Add(endpoint.Close());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Endpoint listen error: {ErrorMessage}", ex.Message);
            }
        }
        await Task.WhenAll(shutdownTasks).ContinueWith(_ => _logger.LogDebug("Listeners closed"));
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
