using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using SoEx.Topology;

namespace SoEx.Transport.ASBTopic
{
    public class ASBTopicEventBinding<I> : Topology.Binding
    {
        private readonly TopicConfig _topicConfig;

        [SetsRequiredMembers]
        public ASBTopicEventBinding(string subsystem, TopicConfig config)
        {
            SubSystem = subsystem;
            Contract = typeof(I);
            Transport = new ASBTopicEventTransport() { Address = AddressFrom(config) };
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
