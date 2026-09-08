using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using SoEx.Abstractions;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.NamedPipe
{
    public class NamedPipeEndpoint<I> : IEndpoint where I : class
    {
        NamedPipeBinding<I>? _binding;
        IIpcServer? namedPipeServer;
        IEndpointPipeline _endpointPipeLine;



        public NamedPipeEndpoint(IEndpointPipeline endpointPipeLine)
        {
            _endpointPipeLine = endpointPipeLine;
        }

        public void Bind(Binding binding, string componentName)
        {
            if (binding is NamedPipeBinding<I> namedPipeBinding)
            {
                _binding = namedPipeBinding;
            }
        }

        public Task Listen()
        {
            namedPipeServer = ServerFactory.Create(NamedPipeCallbackAsync, new ServerFactory.ServerOptions()
            {
                Name = _binding?.Transport.Address.Uri.Host,
                AllowMultipleClients = true,
            });
            return Task.CompletedTask;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        private async Task NamedPipeCallbackAsync(Stream stream)
        {
            var ss = new StreamBytes(stream);
            var request = ss.ReadBytes();
            var response = await DispatchAsync(request);
            ss.WriteBytes(response);
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
