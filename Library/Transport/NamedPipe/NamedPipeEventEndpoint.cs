using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using Autofac;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Transport.NamedPipe
{
    public class NamedPipeEventEndpoint<I> : IEndpoint where I : class
    {
        readonly Channel<byte[]> _channel = System.Threading.Channels.Channel.CreateBounded<byte[]>(1000);
        readonly IMessageSerializer _serializer;
        readonly IHostAndClientLookup _subsystemlifeTimeScope;
        NamedPipeEventBinding<I>? _binding;
        IIpcServer? namedPipeServer;


        public NamedPipeEventEndpoint(IMessageSerializer serializer, IHostAndClientLookup subsystemlifeTimeScope)
        {
            _serializer = serializer;
            _subsystemlifeTimeScope = subsystemlifeTimeScope;
        }

        public void Bind(Binding binding, string componentName)
        {
            if (binding is NamedPipeEventBinding<I> namedPipeBinding)
            {
                _binding = namedPipeBinding;
            }
        }

        public Task Listen()
        {
            var task = Task.Factory.StartNew(ReadChannelAsync, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            namedPipeServer = ServerFactory.Create(NamedPipeCallbackAsync, new ServerFactory.ServerOptions()
            {
                Name = _binding?.Transport.Address.Host,
                AllowMultipleClients = true,
            });
            return Task.CompletedTask;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        private async Task ReadChannelAsync()
        {
            while (await _channel.Reader.WaitToReadAsync())
            {
                var request = await _channel.Reader.ReadAsync();
                await DispatchAsync(request);
            }
        }

        private async Task NamedPipeCallbackAsync(Stream stream)
        {
            var ss = new StreamBytes(stream);
            var request = ss.ReadBytes();
            await _channel.Writer.WriteAsync(request);
        }

        private async Task<byte[]> DispatchAsync(byte[] serializedRequest)
        {
            InvocationRequest request = _serializer.Deserialize<InvocationRequest>(serializedRequest)!;
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(NamedPipeEndpoint<I>)}", ActivityKind.Server, request.ActivityId))
            {
                try
                {
                    var subSystemHost = _subsystemlifeTimeScope.For<ISubSystemHost<I>>();
                    using (Autofac.ILifetimeScope requestScope = subSystemHost.BeginRequestLifetimeScope())
                    {
                        var dispatcher = requestScope.Resolve<IDispatcher>();
                        Debug.Assert(request is not null);
                        var response = await dispatcher.Dispatch<I>(request);
                        return _serializer.Serialize(response);
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
