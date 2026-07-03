using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.Grpc
{
    public static class GrpcExtensions
    {
        public static IServiceCollection RelayClient(this IServiceCollection collection)
        {
            collection.AddSingleton(typeof(GrpcChannel<>), typeof(GrpcChannel<>));
            return collection;
        }
        public static void RelayHost() { }
    }
}
