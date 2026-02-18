using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Net;
using SoEx.Abstractions;
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
