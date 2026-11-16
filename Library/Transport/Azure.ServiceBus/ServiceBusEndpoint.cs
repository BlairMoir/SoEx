using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.Azure.ServiceBus;

public abstract class ServiceBusEndpoint<I> where I : class
{
    protected readonly ILogger _logger;
    readonly IEndpointPipeline _endpointPipeLine;
    protected abstract IBindingPipeline? BindingPipeline { get; }
    private ServiceBusClient? _client;
    private  ServiceBusProcessor? _processor;


    public ServiceBusEndpoint(ILogger logger, IEndpointPipeline endpointPipeLine)
    {
        _endpointPipeLine = endpointPipeLine;
        _logger = logger;
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        await Dispatch(args.Message.Body.ToArray()).ConfigureAwait(false);
        await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, args.Exception.Message);
        return Task.CompletedTask;
    }

    protected async Task StartProcessing(string connectionString, Func<ServiceBusClient, ServiceBusProcessor> createProcessor)
    {
        _client = new ServiceBusClient(connectionString);
        _processor = createProcessor(_client);
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
