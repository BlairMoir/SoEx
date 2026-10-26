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
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(5);
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
            while (!source.IsCancellationRequested)
            {
                try
                {
                    await MessageLoop();
                }
                catch (OperationCanceledException) when (source.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "NATS subscription for {Contract} failed", typeof(I));
                }

                try
                {
                    await Task.Delay(_retryDelay, source.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        private async Task MessageLoop()
        {
            Debug.Assert(_natsClient is not null);
            Debug.Assert(_binding is not null);
            string subject = $"{_binding.SubSystem}-{typeof(I)}";
            int maxConcurrent = _binding.Config.MaxConcurrentMessages;
            using var slots = new SemaphoreSlim(maxConcurrent);

            try
            {
                await foreach (NatsMsg<byte[]> msg in _natsClient.SubscribeAsync<byte[]>(subject,
                                   queueGroup: subject, cancellationToken: source.Token))
                {
                    await slots.WaitAsync(source.Token);
                    _ = HandleMessage(msg, slots);
                }
            }
            finally
            {
                for (int i = 0; i < maxConcurrent; i++)
                {
                    await slots.WaitAsync();
                }
            }
        }

        private async Task HandleMessage(NatsMsg<byte[]> msg, SemaphoreSlim slots)
        {
            await Task.Yield();
            try
            {
                Debug.Assert(msg.Data is not null);
                var dispatchResult = await Dispatch(msg.Data);
                await msg.ReplyAsync(dispatchResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                slots.Release();
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
