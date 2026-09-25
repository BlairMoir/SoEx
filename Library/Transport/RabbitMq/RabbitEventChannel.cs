using System.Diagnostics;
using RabbitMQ.Client;
using SoEx.Topology;


namespace SoEx.Transport.RabbitMq;

public class RabbitEventChannel<I> : SoEx.Topology.IChannel
{
    private RabbitEventBinding<I>? _binding;
    private readonly object _syncChannel = new object();
    private Lazy<Task<Publisher>>? _publisher;
    private CachedString _exchangeName = new CachedString(typeof(I).FullName!);
    private CachedString _routingKey = new CachedString("");

    public async Task<byte[]> InvokeResult(byte[] invocationRequest)
    {
        using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(RabbitEventChannel<I>)}"))
        {
            try
            {
                var publisher = await CurrentPublisher();
                var props = new BasicProperties();
                props.Persistent = true;

                await publisher.Channel.BasicPublishAsync(_exchangeName,_routingKey,true,props,invocationRequest);

                return [];
            }
            catch
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                throw;
            }
        }
    }

    private async Task<Publisher> CurrentPublisher()
    {
        var lazyPublisher = _publisher;
        var currentPublisher = lazyPublisher?.Value;
        if (currentPublisher is not null && currentPublisher.IsCompletedSuccessfully && !currentPublisher.Result.Channel.IsClosed)
            return currentPublisher.Result;

        if (currentPublisher is not null && !currentPublisher.IsCompleted)
            return await currentPublisher;

        lock (_syncChannel)
        {
            if(ReferenceEquals(_publisher, lazyPublisher))
            {
                if(lazyPublisher is not null && currentPublisher is not null &&  currentPublisher.IsCompletedSuccessfully)
                    _ = currentPublisher.Result.DisposeAsync();
                _publisher = new Lazy<Task<Publisher>>(Open);
            }
            currentPublisher = _publisher!.Value;
        }
        return await currentPublisher;
    }

    private async Task<Publisher> Open()
    {
        ConnectionFactory factory = new ConnectionFactory();
        factory.ClientProvidedName = $"event:{typeof(I).FullName} publisher";
        factory.ConfigureFactory(_binding!.RabbitConfig);
        var connection = await factory.CreateConnectionFromConfig(_binding.RabbitConfig);
        var options = new CreateChannelOptions(true,true);
        var channel = await connection.CreateChannelAsync(options);
        return new Publisher(connection, channel);
    }

    public void Bind(Binding binding)
    {
        if(binding is RabbitEventBinding<I> rabbitBinding)
        {
            _binding = rabbitBinding;
        }
    }

    public IBindingPipeline? Pipeline => _binding?.Pipeline;
    public Type Contract => typeof(I);


    private sealed record Publisher(IConnection Connection, RabbitMQ.Client.IChannel Channel) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await Channel.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
