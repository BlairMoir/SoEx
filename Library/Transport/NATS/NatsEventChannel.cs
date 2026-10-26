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
        private readonly SharedConnections<NatsClient> _connections;

        public Type Contract => typeof(I);

        public NatsEventChannel(IMessageSerializer serializer,  SharedConnections<NatsClient> connections)
        {
            _serializer = serializer;
            _connections = connections;
        }

        public void Bind(Binding binding)
        {
            if (binding is NatsEventBinding<I> natsEventBinding)
            {
                _natsEventBinding = natsEventBinding;
            }
        }

        public IBindingPipeline? Pipeline => _natsEventBinding?.Pipeline;

        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(NatsEventChannel<I>)}"))
            {
                try
                {
                    NatsClient nc = await CreateConnection().ConfigureAwait(false);
                    INatsJSContext js = nc.CreateJetStreamContext();
                    PubAckResponse ack;
                    try
                    {
                        ack = await js.PublishAsync<byte[]>(NatsSubject.For<I>(), payload).ConfigureAwait(false);
                    }
                    catch (NatsJSPublishNoResponseException)
                    {
                        await js.CreateEventStream<I>().ConfigureAwait(false);
                        ack = await js.PublishAsync<byte[]>(NatsSubject.For<I>(), payload).ConfigureAwait(false);
                    }
                    ack.EnsureSuccess();
                    return [];
                }
                catch
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }

        private async Task<NatsClient> CreateConnection()
        {
            Debug.Assert(_natsEventBinding is not null);
            var url = _natsEventBinding.Config.Url;
            var client = await _connections.Get(url,
                () => Connect(url),
                c => c.Connection.ConnectionState == NatsConnectionState.Closed
            ).ConfigureAwait(false);
            return client;
        }

        private static async Task<NatsClient> Connect(string url)
        {
            var client = new  NatsClient(new NatsOpts(){ Url = url});
            try
            {
                await client.ConnectAsync().ConfigureAwait(false);
                return client;
            }
            catch
            {
                await client.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

    }
}
