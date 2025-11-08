using SoExTemplate.Common.Contract;
using SoEx.Context;
namespace SoExTemplate.Common.Policy;

public class ContextFlowPolicy : IContextFlowPolicy
{
    public void Incoming(IAmbientContext source, IAmbientContext destination)
    {
        source.CopyIfExists<CallChainContext>(destination);

        CountContext countContext;
        if (source.Contains<CountContext>())
        {
            countContext = new CountContext(source.Get<CountContext>());
        }
        else
        {
            countContext = new CountContext();
        }
        destination.SetIfNotExists(() => countContext);
        destination.SetIfNotExists(() => new CallChainContext());
    }

    public void Outgoing(IAmbientContext source, IAmbientContext destination)
    {
        source.CopyIfExists<CountContext>(destination);
    }

    public void Copy(IAmbientContext source, IAmbientContext destination)
    {
        source.CopyIfExists<CallChainContext>(destination);
        source.CopyIfExists<CountContext>(destination);
    }
    public IDictionary<string, object> ScopeProperties(IAmbientContext context)
    {
        return new Dictionary<string, object>()
        {
             {"HopCount", context.Get<CountContext>().HopCount},
             {"CallChainId", context.Get<CallChainContext>().CallChainId}
        };
    }
}
