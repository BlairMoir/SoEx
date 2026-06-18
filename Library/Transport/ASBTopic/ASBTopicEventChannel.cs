using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.Intrinsics.X86;
using System.Security.Principal;
using Autofac;
using Azure.Messaging.ServiceBus;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Transport.ASBTopic
{
    public class ASBTopicEventChannel<I> : IChannel where I : class
    {
        readonly IMessageSerializer _serializer;
        ASBTopicEventBinding<I>? _binding;
        private ServiceBusClient? _client;
        ServiceBusSender? _sender;

        public ASBTopicEventChannel(IMessageSerializer serializer)
        {
            _serializer = serializer;
        }

        public void Bind(Binding binding)
        {
            if (binding is ASBTopicEventBinding<I> topicEventBinding)
            {
                _binding = topicEventBinding;
                _client = new ServiceBusClient(_binding.Config.ConnectionString);
                _sender = _client.CreateSender(ASBTopicSubject.For<I>());
            }
        }

        public IPipeline? Pipeline => _binding?.Pipeline;

        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(ASBTopicEventChannel<I>)}"))
            {
                try
                {
                    Debug.Assert(_sender is not null);
                    await _sender.SendMessageAsync(new ServiceBusMessage(new BinaryData(payload)));
                    return [];
                }
                catch
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }
    }
}
