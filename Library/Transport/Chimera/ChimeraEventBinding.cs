using System.Diagnostics.CodeAnalysis;
using SoEx.Messaging.Chimera;
using SoEx.Topology;

namespace SoEx.Transport.Chimera;

public class ChimeraEventBinding<I> : Binding
{
    public ChimeraOptions Options { get; }
    public string Topic { get; }

    [SetsRequiredMembers]
    public ChimeraEventBinding(string subSystem, ChimeraOptions options, string topic)
    {
        Contract = typeof(I);
        SubSystem = subSystem;
        Options = options;
        Topic = topic;
        Transport = new ChimeraEventTransport(){ Address = new Address.Single(new Uri($"soex.chimera://{Topic}")) };
    }
}
