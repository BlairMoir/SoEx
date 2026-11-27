using System.Runtime.CompilerServices;
using Azure.Messaging.ServiceBus;

namespace SoEx.Transport.Azure.ServiceBus
{
    public abstract class ServiceBusConfig
    {
        private static readonly ConditionalWeakTable<Func<ServiceBusClient>, string> s_clientKeys = new();
        public required string Namespace { get; init; }
        public required Func<ServiceBusClient> Client { get; init; }
        public ServiceBusProcessorOptions Processor { get; init; } = new() { MaxConcurrentCalls = 16 };
        public Redelivery Redelivery { get; init; } = Redelivery.AfterLockExpiry;

        internal string ClientKey => s_clientKeys.GetValue(Client, _ => Guid.NewGuid().ToString("N"));
    }
}
