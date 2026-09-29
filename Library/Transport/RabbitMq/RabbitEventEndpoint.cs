using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RabbitMQ.AMQP.Client;

using SoEx.Endpoint;
using SoEx.Topology;


namespace SoEx.Transport.RabbitMq;

public class RabbitEventEndpoint<I> : IEndpoint where I : class
{
    readonly ILogger<RabbitEventEndpoint<I>> _logger;
    private RabbitEventBinding<I>? _binding;
    IEndpointPipeline _endpointPipeLine;
    private string? _subscriber;
    private IConnection? _connection;
    private readonly uint _maxDeliveryAttempts = 5;
    private IConsumer? _consumer;

    public RabbitEventEndpoint(ILogger<RabbitEventEndpoint<I>> logger, IEndpointPipeline endpointPipeLine)
    {
        _endpointPipeLine = endpointPipeLine;
        _logger = logger;
    }

    public async Task Listen()
    {
        _connection = await _binding!.ConnectAsync($"event: {typeof(I).FullName} subscriber:{_subscriber}");
        var exchangeName = typeof(I).FullName!;
        var queueName = $"{typeof(I).FullName!}_{_subscriber}";
        var deadLetterName = $"{queueName}.dead";

        var management = _connection.Management();
        await management.Exchange(exchangeName).Type(ExchangeType.FANOUT).DeclareAsync();
        await management.Exchange(deadLetterName).Type(ExchangeType.FANOUT).DeclareAsync();
        await management.Queue(deadLetterName).Type(QueueType.QUORUM).DeclareAsync();
        await management.Queue(queueName)
            .Type(QueueType.QUORUM)
            .DeadLetterExchange(deadLetterName)
            .Arguments(new Dictionary<object, object>(){ {"x-delivery-limit", _maxDeliveryAttempts - 1} })
            .DeclareAsync();
        await management.Binding().SourceExchange(exchangeName).DestinationQueue(queueName).Key("").BindAsync();
        await management.Binding().SourceExchange(deadLetterName).DestinationQueue(deadLetterName).Key("").BindAsync();

        _consumer = await _connection.ConsumerBuilder().Queue(queueName).MessageHandler(Handle).BuildAndStartAsync();
    }

    private async Task Handle(IContext context, IMessage message)
    {
        try
        {
            await Dispatch((byte[])message.Body());
            context.Accept();
        }
        catch (Exception ex)
        {
            var attempt = message.DeliveryCount() + 1;
            if (attempt < _maxDeliveryAttempts)
            {
                _logger.LogWarning(ex, "Handler for {Event} failed attempt {Attempt} of {MaxDeliveryAttempts}",
                    typeof(I).FullName, attempt, _maxDeliveryAttempts);
            }else
            {
              _logger.LogError(ex,"Handler for {Event} failed {MaxDeliveryAttempts} times",typeof(I).FullName, _maxDeliveryAttempts);
            }
            context.Requeue(new Dictionary<string, object>(), deliveryFailed: true);
        }
    }

    private async Task<byte[]> Dispatch(byte[] serializedRequest)
    {
        using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(RabbitEventEndpoint<I>)}", ActivityKind.Server))
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

    public async Task Close()
    {
        if (_consumer != null)
        {
            await _consumer.CloseAsync();
            _consumer.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
    }

    public void Bind(Binding binding, string componentName)
    {
        if (binding is RabbitEventBinding<I> rabbitBinding)
        {
            _binding = rabbitBinding;
            _subscriber = componentName;
        }
    }
}
