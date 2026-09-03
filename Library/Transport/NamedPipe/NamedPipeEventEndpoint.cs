using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using Autofac;
using SoEx.Abstractions;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.NamedPipe
{
    public class NamedPipeEventEndpoint<I> : IEndpoint where I : class
    {

        readonly Channel<byte[]> _channel = System.Threading.Channels.Channel.CreateBounded<byte[]>(1000);
        readonly IEndpointPipeline _endpointPipeLine;
        NamedPipeEventBinding<I>? _binding;
        IIpcServer? namedPipeServer;


        public NamedPipeEventEndpoint(IEndpointPipeline endpointPipeLine)
        {
            _endpointPipeLine = endpointPipeLine;
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
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(NamedPipeEndpoint<I>)}", ActivityKind.Server))
            {
                try
                {
                    return await _endpointPipeLine.ServicePipeLine<I>(serializedRequest, _binding?.Pipeline, activity);
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
