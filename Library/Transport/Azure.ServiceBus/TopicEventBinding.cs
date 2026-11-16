using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using SoEx.Topology;

namespace SoEx.Transport.Azure.ServiceBus
{
    public record TopicEventBinding<I> : Topology.Binding
    {
        private readonly TopicConfig _topicConfig;

        public TopicEventBinding(string subsystem, TopicConfig config) : base(
            typeof(I),
            new TopicEventTransport() { Address = AddressFrom(config) },
            subsystem
            )
        {
            _topicConfig = config;
        }

        public TopicConfig Config => _topicConfig;

        static Address AddressFrom(TopicConfig config)
        {
            var match = Regex.Match(config.ConnectionString, @"Endpoint=(?<ep>[^;]+)",RegexOptions.IgnoreCase);
            var uri = new Uri(match.Success ? match.Groups["ep"].Value : config.ConnectionString);
            return new Address.Single(uri);
        }
    }
}
