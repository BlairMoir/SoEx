using System.Diagnostics;
using NATS.Client.Core;
using NATS.Net;
using SoEx.Abstractions;
using SoEx.Exceptions;
using SoEx.Topology;

namespace SoEx.Transport.NATS
{
    public class NatsChannel<I> : IChannel where I : class
    {
        readonly IMessageSerializer _serializer;
        NatsBinding<I>? _natsBinding;
        private readonly SharedConnections<NatsClient> _connections;

        public Type Contract => typeof(I);

        public NatsChannel(IMessageSerializer serializer, SharedConnections<NatsClient> connections)
        {
            _serializer = serializer;
            _connections = connections;
        }

        public void Bind(Binding binding)
        {
            if (binding is NatsBinding<I> natsBinding)
            {
                _natsBinding = natsBinding;
            }
        }

        public IBindingPipeline? Pipeline => _natsBinding?.Pipeline;

        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(NatsChannel<I>)}"))
            {
                try
                {
                    Debug.Assert(_natsBinding != null);
                    NatsClient nc = await CreateConnection().ConfigureAwait(false);
                    string natsSubject = $"{_natsBinding.SubSystem}-{typeof(I)}";
                    NatsMsg<byte[]> reply = await nc.RequestAsync<byte[], byte[]>(natsSubject, payload).ConfigureAwait(false);
                    if (NatsServiceError.IsIn(reply))
                    {
                        throw new ServiceException("Error executing service pipeline",
                            new InvalidOperationException("The service reported an error"));
                    }
                    return reply.Data ?? [];
                }
                catch (Exception)
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }

        private async Task<NatsClient> CreateConnection()
        {
            Debug.Assert(_natsBinding is not null);
            NatsConfig config = _natsBinding.Config;
            NatsClient client = await _connections.Get(config.ClientKey(),
                () => Connect(config.ClientOptions()),
                c => c.Connection.ConnectionState == NatsConnectionState.Closed
            ).ConfigureAwait(false);
            return client;
        }

        private static async Task<NatsClient> Connect(NatsOpts options)
        {
            var client = new NatsClient(options);
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
