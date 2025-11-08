using System.Diagnostics;
using Autofac;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.SBQueue
{
    public class SBQueueEndpoint<I> : IEndpoint where I : class
    {
        SBQueueBinding<I>? _sbBinding;
        ILogger<SBQueueEndpoint<I>> _logger;
        ServiceBusClient? client;
        ServiceBusProcessor? processor;
        IEndpointPipeline _endpointPipeLine;


        public SBQueueEndpoint(ILogger<SBQueueEndpoint<I>> logger, IEndpointPipeline endpointPipeLine)
        {
            _logger = logger;
            _endpointPipeLine = endpointPipeLine;

        }

        public void Bind(Binding binding, string componentName)
        {
            if (binding is SBQueueBinding<I> sbBinding)
            {
                _sbBinding = sbBinding;
                client = new ServiceBusClient(_sbBinding.Config.ConnectionString);
                processor = client.CreateProcessor(_sbBinding.Config.Queue, new ServiceBusProcessorOptions());
            }
        }

        public async Task Listen()
        {
            ArgumentNullException.ThrowIfNull(processor, "Binding must be set before listening");
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;
            await processor.StartProcessingAsync();
        }

        public async Task Close()
        {
            if (processor is not null)
            {
                await processor.StopProcessingAsync();
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, args.Exception.Message);
            return Task.CompletedTask;
        }


        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            await Dispatch(args.Message.Body.ToArray());
            await args.CompleteMessageAsync(args.Message);
        }
        private async Task Dispatch(byte[] serializedRequest)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(SBQueueEndpoint<I>)}", ActivityKind.Server))
            {
                try
                {
                    await _endpointPipeLine.ServicePipeLine<I>(serializedRequest, _sbBinding?.Pipeline, activity);
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    _logger.LogError(ex, "{ExceptionMessage}", ex.Message);
                    throw;
                }
            }
        }
    }
}
