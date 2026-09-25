using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SoEx.Endpoint;
using SoEx.Topology;
using IChannel = RabbitMQ.Client.IChannel;

namespace SoEx.Transport.RabbitMq;

public class RabbitEventEndpoint<I> : IEndpoint where I : class
{
    readonly ILogger<RabbitEventEndpoint<I>> _logger;
    private RabbitEventBinding<I>? _binding;
    IEndpointPipeline _endpointPipeLine;
    private string? _subscriber;
    private IConnection? _connection;
    IChannel? _channel;
    private string? _consumerTag;

    public RabbitEventEndpoint(ILogger<RabbitEventEndpoint<I>> logger, IEndpointPipeline endpointPipeLine)
    {
        _endpointPipeLine = endpointPipeLine;
        _logger = logger;
    }

    public async Task Listen()
    {
        ConnectionFactory factory = new ConnectionFactory();
        factory.ClientProvidedName = $"event:{typeof(I).FullName} subscriber:{_subscriber}";
        factory.ConfigureFactory(_binding!.RabbitConfig);

        _connection = await factory.CreateConnectionFromConfig(_binding!.RabbitConfig);
        IChannel channel = await _connection.CreateChannelAsync();
        var exchangeName = typeof(I).FullName!;
        var queueName = $"{typeof(I).FullName!}_{_subscriber}";
        var deadLetterName = $"{queueName}.dead";
        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, durable: true);
        await channel.ExchangeDeclareAsync(deadLetterName, ExchangeType.Fanout, durable: true);

        await channel.QueueDeclareAsync(queue: deadLetterName, true, false, false, new Dictionary<string, object?>()
        {
            { "x-queue-type", "quorum" },
        });

        await channel.QueueDeclareAsync(queue: queueName, true, false, false, new Dictionary<string, object?>()
        {
            { "x-queue-type", "quorum" },
            { "x-delivery-limit", 5 },
            { "x-dead-letter-exchange", deadLetterName },
        });
        await channel.QueueBindAsync(queueName, exchangeName,"");
        await channel.QueueBindAsync(deadLetterName, deadLetterName,"");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (ch, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                await Dispatch(body);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Handler for {Event} failed", typeof(I).Name);
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };
        _channel = channel;
        _consumerTag = await channel.BasicConsumeAsync(queueName, false, consumer);
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
        if (_channel != null && _consumerTag != null)
        {
            await _channel.BasicCancelAsync(_consumerTag);
        }

        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
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
