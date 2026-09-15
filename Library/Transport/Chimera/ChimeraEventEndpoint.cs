using System.Diagnostics;
using System.Reflection;
using SoEx.Endpoint;
using SoEx.Messaging.Chimera;
using SoEx.Topology;

namespace SoEx.Transport.Chimera;

public class ChimeraEventEndpoint<I> : IEndpoint where I : class
{
    private ChimeraEventBinding<I>? _binding;
    private readonly IEndpointPipeline _pipeline;
    private string? _subscriber;
    private SqliteSubscription? _subscription;
    private Task? _listener;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ChimeraEventEndpoint(IEndpointPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public void Bind(Binding binding, string componentName)
    {
        if (binding is ChimeraEventBinding<I> chimeraEventBinding)
        {
            _binding = chimeraEventBinding;
            _subscriber = componentName;
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
                    await _subscription.FailAsync(eventEnvelope, ex.Message);
                }
            }
        }
        catch (OperationCanceledException)
        {
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
