using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.Grpc
{
    public static class GrpcExtensions
    {
        public static IServiceCollection GrpcClient(this IServiceCollection collection)
        {
            collection.AddSingleton(typeof(GrpcChannel<>), typeof(GrpcChannel<>));
            return collection;
        }

        public static IServiceCollection GrpcHostListenerPerBinding(this IServiceCollection collection)
        {
            collection.AddTransient(typeof(GrpcEndpointListener), typeof(GrpcEndpointListener));
            return collection;
        }

        public static IServiceCollection GrpcHostListenerShared(this IServiceCollection collection)
        {
            collection.AddSingleton(typeof(GrpcEndpointListener), typeof(GrpcEndpointListener));
            return collection;
        }
    }
}
