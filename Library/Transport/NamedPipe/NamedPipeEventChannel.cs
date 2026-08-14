using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.Intrinsics.X86;
using System.Security.Principal;
using Autofac;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Transport.NamedPipe
{
    public class NamedPipeEventChannel<I> : IChannel where I : class
    {
        readonly IMessageSerializer _serializer;
        NamedPipeEventBinding<I>? _namedPipeBinding;

        public NamedPipeEventChannel(IMessageSerializer serializer)
        {
            _serializer = serializer;
        }

        public void Bind(Binding binding)
        {
            if (binding is NamedPipeEventBinding<I> namedPipeBinding)
            {
                _namedPipeBinding = namedPipeBinding;
            }
        }

        public IPipeline? Pipeline => _namedPipeBinding?.Pipeline;

        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(NamedPipeEventChannel<I>)}"))
            {
                try
                {
                    Debug.Assert(_namedPipeBinding is not null);
                    using (var pipeClient = new NamedPipeClientStream(".", _namedPipeBinding.Transport.Address.Host,
                                                PipeDirection.InOut, PipeOptions.None,
                                                TokenImpersonationLevel.None))
                    {
                        await pipeClient.ConnectAsync(3000).ConfigureAwait(false);
                        var ss = new StreamBytes(pipeClient);
                        ss.WriteBytes(payload);
                        return [];
                    }
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
