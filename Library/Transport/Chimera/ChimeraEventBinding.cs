using System.Diagnostics.CodeAnalysis;
using SoEx.Messaging.Chimera;
using SoEx.Topology;

namespace SoEx.Transport.Chimera;

public abstract record ChimeraEventBinding : Binding
{
    protected ChimeraEventBinding(Type contract, Topology.Transport transport, string subSystem,
        ChimeraOptions options, string topic)
        : base(contract, transport, subSystem)
    {
        Options = options;
        Topic = topic;
    }

    public ChimeraOptions Options { get; init; }
    public string Topic { get; }
}

public record ChimeraEventBinding<I> : ChimeraEventBinding
{
    public ChimeraEventBinding(string subSystem, ChimeraOptions options, string topic) : base(
        typeof(I),
            new ChimeraEventTransport(){ Address = new Address.Single(new Uri($"soex.chimera://{topic}"))},
                subSystem,
            options,
            topic
        )
    {
    }
}
