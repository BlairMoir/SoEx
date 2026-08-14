using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Intrinsics.X86;
using System.Security.Principal;
using Autofac;
using NATS.Client.Core;
using NATS.Net;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Transport.NATS
{
    public class NatsChannel<I> : IChannel where I : class
    {
        readonly IMessageSerializer _serializer;
        NatsBinding<I>? _natsBinding;

        public NatsChannel(IMessageSerializer serializer)
        {
            _serializer = serializer;
        }

        public void Bind(Binding binding)
        {
            if (binding is NatsBinding<I> natsBinding)
            {
                _natsBinding = natsBinding;
            }
        }

        public IPipeline? Pipeline => _natsBinding?.Pipeline;

        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(NatsChannel<I>)}"))
            {
                try
                {
                    Debug.Assert(_natsBinding is not null);
                    NatsClient nc = new NatsClient();
                    await using (nc.ConfigureAwait(false))
                    {
                        string natsSubject = $"{_natsBinding.SubSystem}-{typeof(I)}";
                        NatsMsg<byte[]> reply = await nc.RequestAsync<byte[], byte[]>(natsSubject, payload).ConfigureAwait(false);
                        return reply.Data ?? [];
                    }
                }
                catch (Exception)
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }

    }
}
