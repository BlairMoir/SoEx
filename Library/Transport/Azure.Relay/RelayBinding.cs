using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Relay;
using SoEx.Topology;

namespace SoEx.Transport.Azure.Relay
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
            if (config.TokenProvider is null &&
                (config.KeyName is null || config.Key is null))
            {
                throw new ArgumentException("Relay requires TokenProvider or both KeyName and Key", nameof(config));
            }
            _config = config;
        }

        internal TokenProvider CreateTokenProvider()
        {
            if (_config.TokenProvider is not null)
            {
                return _config.TokenProvider;
            }
            return TokenProvider.CreateSharedAccessSignatureTokenProvider(_config.KeyName, _config.Key);
        }

    }
}
