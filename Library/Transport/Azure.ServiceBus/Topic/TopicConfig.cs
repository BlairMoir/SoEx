using Azure.Messaging.ServiceBus.Administration;

namespace SoEx.Transport.Azure.ServiceBus
{
    public class TopicConfig : ServiceBusConfig
    {
       public Func<ServiceBusAdministrationClient>? Administration { get; init; }
    }
}
