using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Transport.SBQueue
{
    public class SBQueueChannel<I> : IChannel where I : class
    {
        SBQueueBinding<I>? _sbBinding;
        readonly IMessageSerializer _serializer;
        private readonly SharedConnections<ServiceBusClient> _connections;


        public Type Contract => typeof(I);

        public SBQueueChannel(IMessageSerializer serializer, SharedConnections<ServiceBusClient> connections)
        {
            _serializer = serializer;
            _connections = connections;
        }

        public void Bind(Binding binding)
        {
            if (binding is SBQueueBinding<I> sbBinding)
            {
                if (_sbBinding is null)
                {
                    _sbBinding = sbBinding;
                }
            }
        }

        public IBindingPipeline? Pipeline => _sbBinding?.Pipeline;


        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(SBQueueChannel<I>)}"))
            {
                try
                {
                    var sender = await CreateSender().ConfigureAwait(false);
                    await using (sender.ConfigureAwait(false))
                    {
                        await sender.SendMessageAsync(new ServiceBusMessage(new BinaryData(payload)))
                            .ConfigureAwait(false);
                    }
                    return [];
                }
                catch
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }

        private async Task<ServiceBusSender> CreateSender()
        {
            Debug.Assert(_sbBinding != null);
            var connectionString = _sbBinding.Config.ConnectionString;
            var client = await _connections.Get( connectionString,
                    () => Task.FromResult(new ServiceBusClient(connectionString)),
                    c => c.IsClosed ).
                ConfigureAwait(false);
            var sender = client.CreateSender(_sbBinding.Config.Queue);
            return sender;
        }
    }
}
