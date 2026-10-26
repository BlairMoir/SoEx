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
        private readonly SharedConnections<ServiceBusSender> _senders;


        public Type Contract => typeof(I);

        public SBQueueChannel(IMessageSerializer serializer, SharedConnections<ServiceBusClient> connections, SharedConnections<ServiceBusSender> senders)
        {
            _serializer = serializer;
            _connections = connections;
            _senders = senders;
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
                    var sender = await Sender().ConfigureAwait(false);
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
            var connectionString = _sbBinding.Config.ConnectionString;
            string entity = _sbBinding.Config.Queue;
            var sender = await _senders.Get($"{entity}|{connectionString}",
                    () => CreateSender(connectionString, entity),
                    c => c.IsClosed ).
                ConfigureAwait(false);
            return sender;
        }

        private async Task<ServiceBusSender> CreateSender(string connectionString, string entity)
        {
            ServiceBusClient client = await _connections.Get(connectionString,
                () => Task.FromResult(new ServiceBusClient(connectionString)),
                c => c.IsClosed)
                .ConfigureAwait(false) ;
            var sender = client.CreateSender(entity);
            return sender;
        }
    }
}
