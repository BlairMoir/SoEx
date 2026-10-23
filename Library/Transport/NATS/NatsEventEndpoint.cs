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
                _logger.LogError(ex, "NATS event subscription for {Contract} stopped", typeof(I));
            }
        }

        private async Task MessageLoop()
        {
            if (_natsClient is null || _queueGroup is null)
                return;

            INatsJSContext js = _natsClient.CreateJetStreamContext();
            await js.CreateStreamAsync(new StreamConfig(name: NatsSubject.For<I>(), subjects: [NatsSubject.For<I>()]));
            INatsJSConsumer consumer = await js.CreateOrUpdateConsumerAsync(NatsSubject.For<I>(), new ConsumerConfig(_queueGroup){ MaxDeliver =  _maxDeliveryAttempts });

            await foreach (NatsJSMsg<byte[]> msg in consumer.ConsumeAsync<byte[]>(cancellationToken: source.Token))
            {
                try
                {
                    Debug.Assert(msg.Data is not null);
                    await Dispatch(msg.Data);
                    await msg.AckAsync();
                }
                catch (OperationCanceledException) when (source.IsCancellationRequested)
                {
                    break;
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
