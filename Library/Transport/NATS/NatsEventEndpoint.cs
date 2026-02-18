using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Net;
using SoEx.Abstractions;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.NATS
{
    public class NatsEventEndpoint<I> : IEndpoint where I : class
    {
        readonly ILogger<NatsEventEndpoint<I>> _logger;
        private NatsEventBinding<I>? _binding;
        IEndpointPipeline _endpointPipeLine;
        private INatsClient? _natsClient;
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
            _natsClient = new NatsClient();
            Task.Factory.StartNew(Subscribe, TaskCreationOptions.LongRunning);
            return Task.CompletedTask;
        }

        public Task Close()
        {
            source.Cancel();
            // create a mechanism to wait for the subscribe loop to be finished
            return Task.CompletedTask;
        }

        private async Task Subscribe()
        {
            if (_natsClient is null || _queueGroup is null)
                return;

            INatsJSContext js = _natsClient.CreateJetStreamContext();
            await js.CreateStreamAsync(new StreamConfig(name: NatsSubject.For<I>(), subjects: [NatsSubject.For<I>()]));
            INatsJSConsumer consumer = await js.CreateConsumerAsync(NatsSubject.For<I>(), new ConsumerConfig(_queueGroup));

            await foreach (NatsJSMsg<byte[]> msg in consumer.ConsumeAsync<byte[]>(cancellationToken: source.Token))
            {
                try
                {
                    Debug.Assert(msg.Data is not null);
                    await Dispatch(msg.Data);
                    await msg.AckAsync();
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
