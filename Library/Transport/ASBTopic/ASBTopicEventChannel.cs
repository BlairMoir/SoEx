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

        public Type Contract => typeof(I);


        public ASBTopicEventChannel(IMessageSerializer serializer, SharedConnections<ServiceBusClient> connections)
        {
            _serializer = serializer;
            _connections = connections;
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
                    var sender = await CreateSender().ConfigureAwait(false);
                    await using (sender.ConfigureAwait(false))
                    {
                        await sender.SendMessageAsync(new ServiceBusMessage(new BinaryData(payload))).ConfigureAwait(false);
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
            Debug.Assert(_binding != null);
            var connectionString = _binding.Config.ConnectionString;
            var client = await _connections.Get( connectionString,
                    () => Task.FromResult(new ServiceBusClient(connectionString)),
                    c => c.IsClosed ).
                ConfigureAwait(false);
            var sender = client.CreateSender(ASBTopicSubject.For<I>());
            return sender;
        }
    }
}
