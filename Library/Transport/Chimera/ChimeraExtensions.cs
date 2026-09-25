using Microsoft.Extensions.DependencyInjection;
using SoEx.Messaging.Chimera;
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

    public static Binding ToChimeraBinding(this Type contract, ChimeraOptions options)
    {
        var binding = (Binding)Activator.CreateInstance(typeof(ChimeraEventBinding<>).MakeGenericType(contract),
            contract.Name, options, contract.Name)!;
        return binding;
    }
}
