using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SoEx.Endpoint;
using SoEx.Messaging.Chimera;
using SoEx.Topology;

namespace SoEx.Transport.Chimera;

public class ChimeraEventEndpoint
{
    public ChimeraOptions? ChimeraOptions { get; protected set; }
    public string? Topic { get; protected set; }
}

public class ChimeraEventEndpoint<I> : ChimeraEventEndpoint, IEndpoint where I : class
{
    private static readonly TimeSpan Retry = TimeSpan.FromSeconds(1);

    private ChimeraEventBinding<I>? _binding;
    private readonly ILogger _logger;
    private readonly IEndpointPipeline _pipeline;
    private string? _subscriber;
    private SqliteSubscription? _subscription;
    private Task? _listener;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ChimeraEventEndpoint(ILogger<ChimeraEventEndpoint<I>> logger, IEndpointPipeline pipeline)
    {
        _logger = logger;
        _pipeline = pipeline;
    }

    public void Bind(Binding binding, string componentName)
    {
        if (binding is ChimeraEventBinding<I> chimeraEventBinding)
        {
            _binding = chimeraEventBinding;
            _subscriber = componentName;
            ChimeraOptions = chimeraEventBinding.Options;
            Topic = chimeraEventBinding.Topic;
        }
    }

    public Task Listen()
    {
        if(_binding == null || _subscriber == null)
            return Task.CompletedTask;

        _subscription = SqliteSubscription.Open(_subscriber, _binding.Topic, _binding.Options,
            new SubscriptionOptions(){ PollInterval =  TimeSpan.FromMilliseconds(20) });

        _listener = Task.Factory.StartNew(Process, TaskCreationOptions.LongRunning).Unwrap();

        return Task.CompletedTask;
    }

    public async Task Close()
    {
        await _cancellationTokenSource.CancelAsync();

        if (_listener != null)
            await _listener.ConfigureAwait(false);

        if(_subscription != null)
            await _subscription.DisposeAsync().ConfigureAwait(false);
    }

    public async Task Process()
    {
        CancellationToken token = _cancellationTokenSource.Token;
        while (!token.IsCancellationRequested)
        {
            try
            {
                await foreach (EventEnvelope eventEnvelope in _subscription!.ReadAsync(_cancellationTokenSource.Token))
                {
                    try
                    {
                        await Dispatch(eventEnvelope.Payload.ToArray()).ConfigureAwait(false);
                        await _subscription.CompleteAsync(eventEnvelope).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Chimera event for {Contract} failed on attempt {Attempt}",
                            typeof(I), eventEnvelope.Attempt);
                        await _subscription.FailAsync(eventEnvelope, ex.Message);
                    }
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chimera delivery for {Contract} failed: retrying in {Retry}",
                    typeof(I), Retry);
                try
                {
                    await Task.Delay(Retry, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    private async Task Dispatch(byte[] payload)
    {
        using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(ChimeraEventEndpoint<I>)}", ActivityKind.Server))
        {
            try
            {
                await _pipeline.ServicePipeLine<I>(payload, _binding?.Pipeline, activity);
            }
            catch
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                throw;
            }
        }
    }
}
