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
        private ServiceBusClient? client;
        ServiceBusSender? sender;

        public Type Contract => typeof(I);

        public SBQueueChannel(IMessageSerializer serializer)
        {
            _serializer = serializer;
        }

        public void Bind(Binding binding)
        {
            if (binding is SBQueueBinding<I> sbBinding)
            {
                if (_sbBinding is null)
                {
                    _sbBinding = sbBinding;
                    client = new ServiceBusClient(_sbBinding.Config.ConnectionString);
                    sender = client.CreateSender(_sbBinding.Config.Queue);
                }
            }
        }

        public IPipeline? Pipeline => _sbBinding?.Pipeline;


        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(SBQueueChannel<I>)}"))
            {
                try
                {
                    Debug.Assert(sender is not null);
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
    }
}
