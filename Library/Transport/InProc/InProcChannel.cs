using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Transport.InProc
{
    public class InProcChannel<I> : IChannel where I : class
    {
        ILifetimeScope _scope;
        InProcBinding<I>? _inProcBinding;
        readonly InProcListeners _listeners;

        public Type Contract => typeof(I);

        public InProcChannel(ILifetimeScope scope, InProcListeners listeners)
        {
            _scope = scope;
            _listeners = listeners;
        }

        public void Bind(Binding binding)
        {
            if (binding is InProcBinding<I> inProcBinding)
            {
                _inProcBinding = inProcBinding;
            }
        }

        public IPipeline? Pipeline => _inProcBinding?.Pipeline;

        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(InProcChannel<I>)}"))
            {
                try
                {

                    InProcEndpoint<I> host = _listeners.ForAddress<I>(_inProcBinding?.Transport.Address.Uri ?? throw new InvalidOperationException("Binding must be set before a channle is used"));
                    byte[] response = await host.Send(payload).ConfigureAwait(false);
                    return response;
                }
                catch
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }

    }
}
