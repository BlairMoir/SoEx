using Microsoft.Extensions.DependencyInjection;
using SoEx.Topology;

namespace SoEx.Transport.Chimera;

public static class ChimeraExtensions
{
    public static IServiceCollection ChimeraClient(this IServiceCollection collection)
    {
        collection.AddSingleton<ChimeraTopic>();
        collection.AddTransient(typeof(ChimeraEventChannel<>), typeof(ChimeraEventChannel<>));
        return collection;
    }
}
