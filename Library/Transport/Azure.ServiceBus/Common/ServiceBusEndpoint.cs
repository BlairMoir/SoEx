using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.Azure.ServiceBus
{
    public abstract class ServiceBusEndpoint<I> where I : class
    {
        protected readonly ILogger _logger;
        readonly IEndpointPipeline _endpointPipeLine;
        protected abstract IBindingPipeline? BindingPipeline { get; }
        private ServiceBusClient? _client;
        private ServiceBusProcessor? _processor;
        private Redelivery _redelivery;


        public ServiceBusEndpoint(ILogger logger, IEndpointPipeline endpointPipeLine)
        {
            _endpointPipeLine = endpointPipeLine;
            _logger = logger;
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                await Dispatch(args.Message.Body.ToArray()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service Bus message for {Contract} failed on delivery {Delivery}",
                    typeof(I), args.Message.DeliveryCount);
                if (_redelivery == Redelivery.Immediate)
                {
                    await args.AbandonMessageAsync(args.Message).ConfigureAwait(false);
                }
                // Redelivery.AfterLockExpiry: if we don't reply Service bus will wait for lock expiry before resending.
                return;
            }
            await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, args.Exception.Message);
            return Task.CompletedTask;
        }

        protected async Task StartProcessing(ServiceBusConfig config, Func<ServiceBusClient, ServiceBusProcessorOptions, ServiceBusProcessor> createProcessor)
        {
            if (config.Processor.ReceiveMode == ServiceBusReceiveMode.ReceiveAndDelete)
            {
                throw new InvalidOperationException("Receive and delete will lose messages, configure as PeekLock");
            }

            config.Processor.AutoCompleteMessages = false;
            _redelivery = config.Redelivery;
            _client = config.Client();
            _processor = createProcessor(_client, config.Processor);
            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
            await _processor.StartProcessingAsync().ConfigureAwait(false);
        }

        protected async Task StopProcessing()
        {
            if (_processor is not null)
            {
                await _processor.StopProcessingAsync().ConfigureAwait(false);
                await _processor.DisposeAsync().ConfigureAwait(false);
                _processor = null;
            }

            if (_client is not null)
            {
                await _client.DisposeAsync().ConfigureAwait(false);
                _client = null;
            }
        }

        private async Task<byte[]> Dispatch(byte[] serializedRequest)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{GetType()}", ActivityKind.Server))
            {
                try
                {
                    return await _endpointPipeLine.ServicePipeLine<I>(serializedRequest, BindingPipeline, activity).ConfigureAwait(false);
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
