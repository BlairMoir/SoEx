using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.SQS
{
    public record SQSBinding<I> : Topology.Binding
    {
        private readonly SQSConfig _config;

        public SQSBinding(SQSConfig config) : base(
            typeof(I),
            new SQSTransport() { Address = new Address.Single(new Uri(config.QueueUrl)) },
            "NotSet"
            )
        {
            _config = config;
        }

        public SQSConfig Config => _config;
    }
}
