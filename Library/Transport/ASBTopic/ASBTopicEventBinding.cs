using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

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

        static Uri AddressFrom(TopicConfig config)
        {
            var match = Regex.Match(config.ConnectionString, @"Endpoint=(?<ep>[^;]+)",RegexOptions.IgnoreCase);
            return new Uri(match.Success ? match.Groups["ep"].Value : config.ConnectionString);
        }
    }
}
