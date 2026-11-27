using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.Azure.ServiceBus
{
    public class TopicEventEndpoint<I> : ServiceBusEndpoint<I>, IEndpoint where I : class
    {
        private TopicEventBinding<I>? _binding;
        private string? _subscriber;

        public TopicEventEndpoint(ILogger<TopicEventEndpoint<I>> logger, IEndpointPipeline endpointPipeLine) :
            base(logger, endpointPipeLine)
        {
        }

        protected override IBindingPipeline? BindingPipeline => _binding?.Pipeline;

        public void Bind(Binding binding, string componentName)
        {
            if (binding is TopicEventBinding<I> topicEventBinding)
            {
                _binding = topicEventBinding;
                _subscriber = componentName;
            }
        }

        public async Task Listen()
        {
            if (_binding is null)
                return;

            TopicConfig config = _binding.Config;
            string topicName = TopicSubject.For<I>();
            await ConfigureServiceBus(config, topicName);
            await StartProcessing(config,
                    (client, options) => client.CreateProcessor(topicName, _subscriber, options)
                );
        }

        public async Task Close()
        {
            await StopProcessing();
        }

        private async Task ConfigureServiceBus(TopicConfig config, string topicName)
        {
            if (config.Administration is null)
                return;

            try
            {
                ServiceBusAdministrationClient adminClient = config.Administration();
                if (!await adminClient.TopicExistsAsync(topicName))
                {
                    await adminClient.CreateTopicAsync(topicName);
                }

                if (!await adminClient.SubscriptionExistsAsync(topicName, _subscriber))
                {
                    await adminClient.CreateSubscriptionAsync(topicName, _subscriber);
                }
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
                _logger.LogDebug("Topic/Subscription already exists topic:{Topic} subscriber:{Subscriber}", topicName, _subscriber);
            }
            catch (Exception)
            {
                _logger.LogWarning("Could not check Topic/Subscription exists topic:{Topic} subscriber:{Subscriber}", topicName, _subscriber);
            }
        }
    }
}
