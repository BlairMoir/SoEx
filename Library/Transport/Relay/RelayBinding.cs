using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Relay;

namespace SoEx.Relay
{
    public class RelayBinding<I> : Topology.Binding
    {
        private readonly RelayConfig _config;
        [SetsRequiredMembers]
        public RelayBinding(string subsystem, RelayConfig config)
        {
            SubSystem = subsystem;
            Contract = typeof(I);
            Transport = new RelayTransport() { Address = new Uri(string.Format("sb://{0}/{1}", config.RelayNamespace, config.ConnectionName)) };
            _config = config;
        }

        internal TokenProvider CreateTokenProvider()
        {
            return TokenProvider.CreateSharedAccessSignatureTokenProvider(_config.KeyName, _config.Key);
        }

    }
}
