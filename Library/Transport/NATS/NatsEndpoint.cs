using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Net;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.NATS
{
    public class NatsEndpoint<I> : IEndpoint where I : class
    {
        readonly ILogger<NatsEndpoint<I>> _logger;
        NatsBinding<I>? _binding;
        IEndpointPipeline _endpointPipeLine;
        private INatsClient? _natsClient;
        private Task? _subscription;
        CancellationTokenSource source = new CancellationTokenSource();


        public NatsEndpoint(ILogger<NatsEndpoint<I>> logger, IEndpointPipeline endpointPipeLine)
        {
            _endpointPipeLine = endpointPipeLine;
            _logger = logger;
        }

        public void Bind(Binding binding, string componentName)
        {
            if (binding is NatsBinding<I> namedPipeBinding)
            {
                _binding = namedPipeBinding;
            }
        }

        public Task Listen()
        {
            Debug.Assert(_binding is not null);
            _natsClient = new NatsClient(new NatsOpts(){ Url = _binding.Config.Url });
            _subscription = Task.Run(Subscribe);
            return Task.CompletedTask;
        }

        public async Task Close()
        {
            await source.CancelAsync();
            if(_subscription is not null)
                await _subscription.ConfigureAwait(false);

            if (_natsClient is not null)
            {
                await _natsClient.DisposeAsync().ConfigureAwait(false);
                _natsClient = null;
            }
        }

        private async Task Subscribe()
        {
            try
            {
                await MessageLoop();
            }
            catch (OperationCanceledException) when (source.IsCancellationRequested)
            {
                // stopping
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NATS subscription for {Contract} stopped", typeof(I));
            }
        }

        private async Task MessageLoop()
        {
            Debug.Assert(_natsClient is not null);
            Debug.Assert(_binding is not null);
            string subject = $"{_binding.SubSystem}-{typeof(I)}";
            await foreach (NatsMsg<byte[]> msg in _natsClient.SubscribeAsync<byte[]>(subject, cancellationToken: source.Token))
            {
                try
                {
                    Debug.Assert(msg.Data is not null);
                    var dispatchResult = await Dispatch(msg.Data);
                    await msg.ReplyAsync(dispatchResult, cancellationToken: source.Token);
                }
                catch (OperationCanceledException) when (source.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }


        private async Task<byte[]> Dispatch(byte[] serializedRequest)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(NatsEndpoint<I>)}", ActivityKind.Server))
            {
                try
                {
                    return await _endpointPipeLine.ServicePipeLine<I>(serializedRequest, _binding?.Pipeline, activity);
                }
                catch
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }
    }
}
