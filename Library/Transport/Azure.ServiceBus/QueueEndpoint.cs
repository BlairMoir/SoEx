using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.Azure.ServiceBus
{
    public class QueueEndpoint<I> : ServiceBusEndpoint<I>, IEndpoint where I : class
    {
        QueueBinding<I>? _sbBinding;

        public QueueEndpoint(ILogger<QueueEndpoint<I>> logger, IEndpointPipeline endpointPipeLine) :
            base(logger, endpointPipeLine)
        {
        }

        protected override IBindingPipeline? BindingPipeline => _sbBinding?.Pipeline;

        public void Bind(Binding binding, string componentName)
        {
            if (binding is QueueBinding<I> sbBinding)
            {
                _sbBinding = sbBinding;
            }
        }

        public async Task Listen()
        {
            if (_sbBinding is null)
                return;

            var config = _sbBinding.Config;
            var options = new ServiceBusProcessorOptions()
            {
                MaxConcurrentCalls = config.MaxConcurrentCalls,
            };

            await StartProcessing(config.ConnectionString,
                client => client.CreateProcessor(config.Queue, options)
                );
        }

        public async Task Close()
        {
            await StopProcessing();
        }
    }
}
