
using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;
using SoEx.Transport.Grpc.Protection;

namespace SoEx.Transport.Grpc
{
    public record GrpcBinding<I> : Topology.Binding
    {
        private readonly GrpcConfig[] _config;

        public GrpcBinding(string subsystem, GrpcConfig[] config) : base(
            typeof(I),
                new GrpcTransport() { Address = TransportAddress(config) },
                subsystem
            )
        {
            _config = config;
        }

        public GrpcConfig[] Config => _config;

        private static Address TransportAddress(GrpcConfig[] config)
        {
            List<Uri> uriList = new List<Uri>();
            foreach (var configItem in config)
            {
                string addressScheme = configItem.Protection switch
                {
                    ClearTextGrpc => "http",
                    GrpcCertificate => "https",
                    GrpcCertificateFromPath => "https",
                    _ => throw new ArgumentOutOfRangeException()
                };

                var uri = new Uri($"{addressScheme}://{configItem.Host}:{configItem.Port}");
                uriList.Add(uri);
            }

            return new Address.Many([..uriList]);
        }
    }
}
