using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Relay
{
    public static class RelayExtensions
    {
        public static IServiceCollection RelayClient(this IServiceCollection collection)
        {
            collection.AddSingleton(typeof(RelayChannel<>), typeof(RelayChannel<>));
            return collection;
        }
        public static void RelayHost() { }
    }
}
