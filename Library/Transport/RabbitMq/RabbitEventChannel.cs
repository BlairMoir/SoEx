using System.Collections.Concurrent;
using System.Diagnostics;
using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using SoEx.Topology;


namespace SoEx.Transport.RabbitMq
{
    public class RabbitEventChannel<I> : SoEx.Topology.IChannel
    {
        private RabbitEventBinding<I>? _binding;
        private static readonly ConcurrentDictionary<RabbitPublisherKey, SharedPublisher> s_shared = new();
        private SharedPublisher? _shared;
        private static readonly TimeSpan s_reconectWait = TimeSpan.FromSeconds(30);

        public async Task<byte[]> InvokeResult(byte[] invocationRequest)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(RabbitEventChannel<I>)}"))
            {
                try
                {
                    Publisher publisher = await CurrentPublisher();
                    await WhileReconnectiong(publisher.Sender);
                    PublishResult result = await publisher.Sender.PublishAsync(new AmqpMessage(invocationRequest).Durable(true));

                    if (result.Outcome.State == OutcomeState.Accepted)
                        return [];

                    if (result.Outcome.State == OutcomeState.Released)
                    {
                        throw new InvalidOperationException($"No subscriber for {typeof(I).FullName}");
                    }

                    throw new InvalidOperationException($"Broker rejected {typeof(I).FullName}: {result.Outcome.Error}");
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
            SharedPublisher shared = _shared!;
            Lazy<Task<Publisher>>? lazyPublisher = shared.Publisher;
            Task<Publisher>? currentPublisher = lazyPublisher?.Value;
            if (currentPublisher is not null && currentPublisher.IsCompletedSuccessfully && currentPublisher.Result.Sender.State != State.Closed)
                return currentPublisher.Result;

            if (currentPublisher is not null && !currentPublisher.IsCompleted)
                return await currentPublisher;

            lock (shared)
            {
                if (ReferenceEquals(shared.Publisher, lazyPublisher))
                {
                    if (lazyPublisher is not null && currentPublisher is not null && currentPublisher.IsCompletedSuccessfully)
                        _ = currentPublisher.Result.DisposeAsync();
                    shared.Publisher = new Lazy<Task<Publisher>>(Open);
                }
                currentPublisher = shared.Publisher!.Value;
            }
            return await currentPublisher;
        }

        private static async Task WhileReconnectiong(IPublisher sender)
        {
            DateTime deadline = DateTime.UtcNow + s_reconectWait;
            while (sender.State == State.Reconnecting && DateTime.UtcNow < deadline)
            {
                await Task.Delay(100);
            }
        }

        private async Task<Publisher> Open()
        {
            IConnection connection = await _binding!.ConnectAsync($"event:{typeof(I).FullName} publisher");
            try
            {
                IPublisher sender = await connection.PublisherBuilder().Exchange(typeof(I).FullName!).Key("").BuildAsync();
                return new Publisher(connection, sender);
            }
            catch
            {
                await connection.CloseAsync();
                connection.Dispose();
                throw;
            }
        }

        public void Bind(Binding binding)
        {
            if (binding is RabbitEventBinding<I> rabbitBinding)
            {
                _binding = rabbitBinding;
                _shared = s_shared.GetOrAdd(Key(rabbitBinding), _ => new SharedPublisher());
            }
        }

        private static RabbitPublisherKey Key(RabbitEventBinding<I> binding)
        {
            IEnumerable<string> credentials = binding.RabbitConfig.Select(config => $"{config.UserName}|{config.Password}");
            string hosts = $"{string.Join(",", binding.Transport.Address.Uris)}|{string.Join(", ", credentials)}";
            return new RabbitPublisherKey(hosts, binding.Connection);
        }

        public IBindingPipeline? Pipeline => _binding?.Pipeline;
        public Type Contract => typeof(I);

        private sealed class SharedPublisher
        {
            public Lazy<Task<Publisher>>? Publisher;
        }

        private sealed record Publisher(IConnection Connection, IPublisher Sender) : IAsyncDisposable
        {
            public async ValueTask DisposeAsync()
            {
                await Sender.CloseAsync();
                Sender.Dispose();
                await Connection.CloseAsync();
                Connection.Dispose();
            }
        }
    }
}
