using System.Diagnostics;
using SoEx.Topology;

namespace SoEx.Transport.Chimera;

public class ChimeraEventChannel<I> : IChannel
{
    public Type Contract => typeof(I);
    private ChimeraEventBinding<I>? _binding;
    public IPipeline? Pipeline => _binding?.Pipeline;

    public void Bind(Binding binding)
    {
        if (binding is ChimeraEventBinding<I> chimeraEventBinding)
        {
            _binding = chimeraEventBinding;
        }
    }


    public async Task<byte[]> InvokeResult(byte[] invocationRequest)
    {
        using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(ChimeraEventChannel<I>)}"))
        {
            try
            {
                ArgumentNullException.ThrowIfNull(_binding);
                var topic = ChimeraTopic.For(_binding.Options, _binding.Topic);
                await topic.AppendAsync(invocationRequest).ConfigureAwait(false);
                return [];
            }
            catch
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                throw;
            }
        }
    }
}
