using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Relay;
using SoEx.Topology;

namespace SoEx.Relay
{
    public record RelayBinding<I> : Topology.Binding
    {
        private readonly RelayConfig _config;
        public RelayBinding(string subsystem, RelayConfig config) : base(
            typeof(I),
            new RelayTransport() { Address = new Address.Single(new Uri(string.Format("sb://{0}/{1}", config.RelayNamespace, config.ConnectionName))) },
            subsystem
            )
        {
            _config = config;
        }

        internal TokenProvider CreateTokenProvider()
        {
            return TokenProvider.CreateSharedAccessSignatureTokenProvider(_config.KeyName, _config.Key);
        }

    }
}
