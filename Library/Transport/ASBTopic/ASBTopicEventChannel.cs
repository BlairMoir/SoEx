using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Transport.ASBTopic
{
    public class ASBTopicEventChannel<I> : IChannel where I : class
    {
        readonly IMessageSerializer _serializer;
        ASBTopicEventBinding<I>? _binding;
        private readonly SharedConnections<ServiceBusClient> _connections;
        private readonly SharedConnections<ServiceBusSender> _senders;

        public Type Contract => typeof(I);


        public ASBTopicEventChannel(IMessageSerializer serializer, SharedConnections<ServiceBusClient> connections, SharedConnections<ServiceBusSender> senders)
        {
            _serializer = serializer;
            _connections = connections;
            _senders = senders;
        }

        public void Bind(Binding binding)
        {
            if (binding is ASBTopicEventBinding<I> topicEventBinding)
            {
                _binding = topicEventBinding;
            }
        }

        public IBindingPipeline? Pipeline => _binding?.Pipeline;

        public async Task<byte[]> InvokeResult(byte[] payload)
        {


            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(ASBTopicEventChannel<I>)}"))
            {
                try
                {
                    var sender = await Sender().ConfigureAwait(false);
                        await sender.SendMessageAsync(new ServiceBusMessage(new BinaryData(payload))).ConfigureAwait(false);
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
            Debug.Assert(_binding != null);
            var connectionString = _binding.Config.ConnectionString;
            string entity = ASBTopicSubject.For<I>();
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
