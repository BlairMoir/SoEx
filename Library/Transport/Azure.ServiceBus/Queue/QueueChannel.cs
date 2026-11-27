using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Transport.Azure.ServiceBus
{
    public class QueueChannel<I> : IChannel where I : class
    {
        QueueBinding<I>? _sbBinding;
        readonly IMessageSerializer _serializer;
        private readonly SharedConnections<ServiceBusClient> _connections;
        private readonly SharedConnections<ServiceBusSender> _senders;


        public Type Contract => typeof(I);

        public QueueChannel(IMessageSerializer serializer, SharedConnections<ServiceBusClient> connections, SharedConnections<ServiceBusSender> senders)
        {
            _serializer = serializer;
            _connections = connections;
            _senders = senders;
        }

        public void Bind(Binding binding)
        {
            if (binding is QueueBinding<I> sbBinding)
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
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(QueueChannel<I>)}"))
            {
                try
                {
                    ServiceBusSender sender = await Sender().ConfigureAwait(false);
                    await sender.SendMessageAsync(new ServiceBusMessage(new BinaryData(payload)))
                        .ConfigureAwait(false);
                    return [];
                }
                catch
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }

        private async Task<ServiceBusSender> Sender()
        {
            Debug.Assert(_sbBinding != null);
            QueueConfig config = _sbBinding.Config;
            string entity = config.Queue;
            ServiceBusSender sender = await _senders.Get($"{entity}|{config.ClientKey}",
                    () => CreateSender(config, entity),
                    c => c.IsClosed).
                ConfigureAwait(false);
            return sender;
        }

        private async Task<ServiceBusSender> CreateSender(ServiceBusConfig config, string entity)
        {
            ServiceBusClient client = await _connections.Get(config.ClientKey,
                () => Task.FromResult(config.Client()),
                c => c.IsClosed)
                .ConfigureAwait(false);
            ServiceBusSender sender = client.CreateSender(entity);
            return sender;
        }
    }
}
