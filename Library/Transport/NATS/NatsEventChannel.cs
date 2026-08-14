using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.Intrinsics.X86;
using System.Security.Principal;
using Autofac;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Net;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Transport.NATS
{
    public class NatsEventChannel<I> : IChannel where I : class
    {
        readonly IMessageSerializer _serializer;
        NatsEventBinding<I>? _natsEventBinding;

        public NatsEventChannel(IMessageSerializer serializer)
        {
            _serializer = serializer;
        }

        public void Bind(Binding binding)
        {
            if (binding is NatsEventBinding<I> natsEventBinding)
            {
                _natsEventBinding = natsEventBinding;
            }
        }

        public IPipeline? Pipeline => _natsEventBinding?.Pipeline;

        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(NatsEventChannel<I>)}"))
            {
                try
                {
                    NatsClient nc = new NatsClient();
                    await using (nc.ConfigureAwait(false))
                    {
                        INatsJSContext js = nc.CreateJetStreamContext();
                        await js.CreateStreamAsync(new StreamConfig(name: NatsSubject.For<I>(),
                            subjects: [NatsSubject.For<I>()])).ConfigureAwait(false);
                        var ack = await js.PublishAsync<byte[]>(NatsSubject.For<I>(), payload).ConfigureAwait(false);
                        ack.EnsureSuccess();
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
