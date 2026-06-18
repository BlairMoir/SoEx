using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using Autofac;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.ASBTopic
{
    public class ASBTopicEventEndpoint<I> : IEndpoint where I : class
    {
        readonly ILogger<ASBTopicEventEndpoint<I>> _logger;
        private ASBTopicEventBinding<I>? _binding;
        IEndpointPipeline _endpointPipeLine;
        private string? _subscriber;
        private CancellationTokenSource source = new CancellationTokenSource();
        ServiceBusClient? client;
        ServiceBusProcessor? processor;



        public ASBTopicEventEndpoint(ILogger<ASBTopicEventEndpoint<I>> logger, IEndpointPipeline endpointPipeLine)
        {
            _endpointPipeLine = endpointPipeLine;
            _logger = logger;
        }

        public void Bind(Binding binding, string componentName)
        {
            if (binding is ASBTopicEventBinding<I> topicEventBinding)
            {
                _binding = topicEventBinding;
                _subscriber = componentName;
            }
        }

        public async Task Listen()
        {
            if (_binding is null)
                return;

            var connectionString = _binding.Config.ConnectionString;
            var topicName = ASBTopicSubject.For<I>();
            await ConfigureServiceBus(connectionString, topicName);

            client = new ServiceBusClient(connectionString);
            processor = client.CreateProcessor(topicName, _subscriber, new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;
            await processor.StartProcessingAsync();
        }

        public async Task Close()
        {
            if (processor is null)
                return;

            await processor.StopProcessingAsync();
        }

        private async Task ConfigureServiceBus(string connectionString, string topicName)
        {
            try
            {
                var adminClient = new ServiceBusAdministrationClient(connectionString);
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
                _logger.LogDebug("Topic/Subscription already exists topic:{Topic} subscriber:{Subscriber}",topicName, _subscriber);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not check Topic/Subscription exists topic:{Topic} subscriber:{Subscriber}", topicName, _subscriber);
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, args.Exception.Message);
            return Task.CompletedTask;
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            await Dispatch(args.Message.Body.ToArray());
            await args.CompleteMessageAsync(args.Message);
        }

        private async Task<byte[]> Dispatch(byte[] serializedRequest)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(ASBTopicEventEndpoint<I>)}", ActivityKind.Server))
            {
                try
                {
                    return await _endpointPipeLine.ServicePipeLine<I>(serializedRequest, _binding?.Pipeline, activity);
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
