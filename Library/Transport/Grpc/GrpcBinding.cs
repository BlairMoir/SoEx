
using System.Diagnostics.CodeAnalysis;
using SoEx.Transport.Grpc.Protection;

namespace SoEx.Transport.Grpc
{
    public class GrpcBinding<I> : Topology.Binding
    {
        private readonly GrpcConfig _config;
        [SetsRequiredMembers]
        public GrpcBinding(string subsystem, GrpcConfig config)
        {
            SubSystem = subsystem;
            Contract = typeof(I);

            Transport = new GrpcTransport() { Address = TransportAddress(config) };
            _config = config;
        }

        public GrpcConfig Config => _config;

        private Uri TransportAddress(GrpcConfig config)
        {
            string addressScheme = config.Protection switch
            {
                ClearTextGrpc => "http",
                GrpcCertificate => "https",
                GrpcCertificateFromPath => "https",
                _ => throw new ArgumentOutOfRangeException()
            };

            return new Uri($"{addressScheme}://{config.Host}:{config.Port}");
        }
    }
}
