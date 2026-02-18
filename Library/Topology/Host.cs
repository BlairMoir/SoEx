using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Topology
{
    public class Host
    {
        public required Type Implementation { get; init; }
        public required Binding[] Endpoints { get; init; }
        public required Client[] Proxies { get; init; }
        public IServiceCollection? ServiceCollection { get; init; }
    }
}
