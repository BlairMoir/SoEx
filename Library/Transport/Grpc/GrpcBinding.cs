
using System.Diagnostics.CodeAnalysis;
using SoEx.Topology;
using SoEx.Transport.Grpc.Protection;

namespace SoEx.Transport.Grpc
{
    public class GrpcBinding<I> : Topology.Binding
    {
        private readonly GrpcConfig[] _config;
        [SetsRequiredMembers]
        public GrpcBinding(string subsystem, GrpcConfig[] config)
        {
            SubSystem = subsystem;
            Contract = typeof(I);

            Transport = new GrpcTransport() { Address = TransportAddress(config) };
            _config = config;
        }

        public GrpcConfig[] Config => _config;

        private Address TransportAddress(GrpcConfig[] config)
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

            return new Address.Many(uriList.ToArray());
        }
    }
}
