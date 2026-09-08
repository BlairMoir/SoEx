using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;

namespace SoEx.Transport.SQS
{
    public class SQSBinding<I> : Topology.Binding
    {
        private readonly SQSConfig _config;
        [SetsRequiredMembers]
        public SQSBinding(SQSConfig config)
        {
            Contract = typeof(I);
            SubSystem = "NotSet";
            Transport = new SQSTransport() { Address = new Address.Single(new Uri(config.QueueUrl)) };
            _config = config;
        }

        public SQSConfig Config => _config;
    }
}
