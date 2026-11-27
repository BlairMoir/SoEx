using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Topology
{
    public record Host
    {
        public required Type Implementation { get; init; }
        public required ImmutableArray<Binding> Endpoints { get; init; }
        public required ImmutableArray<Client> Proxies { get; init; }
        public IServiceCollection? ServiceCollection { get; init; }
    }
}
