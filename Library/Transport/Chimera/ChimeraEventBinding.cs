using System.Diagnostics.CodeAnalysis;
using SoEx.Messaging.Chimera;
using SoEx.Topology;

namespace SoEx.Transport.Chimera;

public record ChimeraEventBinding : Binding
{
    public required ChimeraOptions Options { get; init; }
    public required string Topic { get; init; }
}

public record ChimeraEventBinding<I> : ChimeraEventBinding
{
    public ChimeraEventBinding(string subSystem, ChimeraOptions options, string topic)
    {
        Contract = typeof(I);
        SubSystem = subSystem;
        Options = options;
        Topic = topic;
        Transport = new ChimeraEventTransport(){ Address = new Address.Single(new Uri($"soex.chimera://{Topic}")) };
    }
}
