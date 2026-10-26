using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Net;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.NATS
{
    public class NatsEventEndpoint<I> : IEndpoint where I : class
    {
        private readonly uint _maxDeliveryAttempts = 5;
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(5);
        readonly ILogger<NatsEventEndpoint<I>> _logger;
        private NatsEventBinding<I>? _binding;
        IEndpointPipeline _endpointPipeLine;
        private INatsClient? _natsClient;
        private Task? _subscription;
        private string? _queueGroup;
        private CancellationTokenSource source = new CancellationTokenSource();


        public NatsEventEndpoint(ILogger<NatsEventEndpoint<I>> logger, IEndpointPipeline endpointPipeLine)
        {
            _endpointPipeLine = endpointPipeLine;
            _logger = logger;
        }

        public void Bind(Binding binding, string componentName)
        {
            if (binding is NatsEventBinding<I> namedPipeBinding)
            {
                _binding = namedPipeBinding;
                _queueGroup = componentName;
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
                catch (OperationCanceledException)
                {
                    // stopping
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "NATS event subscription for {Contract} failed", typeof(I));
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
            Debug.Assert(_binding is not null);
            if (_natsClient is null || _queueGroup is null)
                return;

            INatsJSContext js = _natsClient.CreateJetStreamContext();
            await js.CreateStreamAsync(new StreamConfig(name: NatsSubject.For<I>(), subjects: [NatsSubject.For<I>()]));
            INatsJSConsumer consumer = await js.CreateOrUpdateConsumerAsync(NatsSubject.For<I>(), new ConsumerConfig(_queueGroup){ MaxDeliver =  _maxDeliveryAttempts });

            using var consumerLost = CancellationTokenSource.CreateLinkedTokenSource(source.Token);
            var consumerOption = new NatsJSConsumeOpts()
            {
                MaxMsgs = 1000,
                NotificationHandler = (notification, _) => OnNatsConsumerNotification(notification, consumerLost)
            };


            int maxConcurrent = _binding.Config.MaxConcurrentMessages;
            using var slots = new SemaphoreSlim(maxConcurrent);
            try
            {
                await foreach (NatsJSMsg<byte[]> msg in consumer.ConsumeAsync<byte[]>(opts: consumerOption, cancellationToken: consumerLost.Token))
                {
                    await slots.WaitAsync(consumerLost.Token);
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

        private Task OnNatsConsumerNotification(INatsJSNotification notification, CancellationTokenSource consumerLost)
        {
            if (notification is NatsJSNoRespondersNotification)
            {
                _logger.LogWarning("Nats consumer {Consumer} for {Contract} is gone, retrying", _queueGroup, typeof(I));
                consumerLost.Cancel();
            }

            return Task.CompletedTask;
        }

        private async Task HandleMessage(NatsJSMsg<byte[]> msg, SemaphoreSlim slots)
        {
            await Task.Yield();
            try
            {
                Debug.Assert(msg.Data is not null);
                await Dispatch(msg.Data);
                await msg.AckAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                try
                {
                    ulong attempt = msg.Metadata?.NumDelivered ?? 1;
                    if (attempt < _maxDeliveryAttempts)
                    {
                        await msg.NakAsync(TimeSpan.FromSeconds(attempt));
                    }
                    else
                    {
                        await msg.AckTerminateAsync();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }
            finally
            {
                slots.Release();
            }
        }


        private async Task<byte[]> Dispatch(byte[] serializedRequest)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(NatsEventEndpoint<I>)}", ActivityKind.Server))
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
