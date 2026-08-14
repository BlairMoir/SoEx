using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Principal;
using Autofac;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Transport.NamedPipe
{
    public class NamedPipeChannel<I> : IChannel where I : class
    {
        readonly IMessageSerializer _serializer;
        NamedPipeBinding<I>? _namedPipeBinding;

        public NamedPipeChannel(IMessageSerializer serializer)
        {
            _serializer = serializer;
        }

        public void Bind(Binding binding)
        {
            if (binding is NamedPipeBinding<I> namedPipeBinding)
            {
                _namedPipeBinding = namedPipeBinding;
            }
        }

        public IPipeline? Pipeline => _namedPipeBinding?.Pipeline;

        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(NamedPipeChannel<I>)}"))
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
                        byte[] response = ss.ReadBytes();
                        return response;
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
