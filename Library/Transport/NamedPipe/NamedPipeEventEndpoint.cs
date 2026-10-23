using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.NamedPipe
{
    public class NamedPipeEventEndpoint<I> : IEndpoint where I : class
    {

        readonly ILogger<NamedPipeEventEndpoint<I>> _logger;
        private Channel<byte[]> _channel = System.Threading.Channels.Channel.CreateBounded<byte[]>(1000) ;
        readonly IEndpointPipeline _endpointPipeLine;
        NamedPipeEventBinding<I>? _binding;
        IIpcServer? namedPipeServer;
        private Task? _reader;


        public NamedPipeEventEndpoint( ILogger<NamedPipeEventEndpoint<I>> logger ,  IEndpointPipeline endpointPipeLine)
        {
            _logger = logger;
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
            ArgumentNullException.ThrowIfNull(_binding);
            _channel = System.Threading.Channels.Channel.CreateBounded<byte[]>(1000);
            _reader = Task.Run(ReadChannelAsync);
            namedPipeServer = ServerFactory.Create(NamedPipeCallbackAsync, new ServerFactory.ServerOptions()
            {
                Name = _binding.Transport.Address.Uri.Host,
                AllowMultipleClients = true,
            });
            return Task.CompletedTask;
        }

        public async Task Close()
        {
            if (namedPipeServer is not null)
            {
                await namedPipeServer.DisposeAsync();
                namedPipeServer = null;
            }
            _channel.Writer.TryComplete();
            if (_reader is not null)
            {
                await _reader;
                _reader = null;
            }
        }

        private async Task ReadChannelAsync()
        {
            await foreach(byte[] request in _channel.Reader.ReadAllAsync())
            {
                try
                {
                    await DispatchAsync(request);
                }
                catch(Exception exception)
                {
                    _logger.LogError(exception, exception.Message);
                }
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
