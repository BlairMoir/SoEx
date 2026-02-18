using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.InProc
{
    public class InProcEndpoint<I> : IEndpoint where I : class
    {
        readonly ILogger<InProcEndpoint<I>> _logger;
        InProcBinding<I>? _binding;
        readonly InProcListeners _listeners;

        IEndpointPipeline _endpointPipeLine;


        public InProcEndpoint(ILogger<InProcEndpoint<I>> logger, IEndpointPipeline endpointPipeLine, InProcListeners listeners)
        {

            _listeners = listeners;
            _endpointPipeLine = endpointPipeLine;
            _logger = logger;
        }

        public void Bind(Binding binding, string componentName)
        {
            if (binding is InProcBinding<I> inProcBinding)
            {
                _binding = inProcBinding;
            }
        }

        public Task Listen()
        {
            ArgumentNullException.ThrowIfNull(_binding, "Binding must be set before listening");
            _listeners.Register(_binding.Transport.Address, this);
            return Task.CompletedTask;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        internal Task<byte[]> Send(byte[] serializedRequest)
        {
            return Dispatch(serializedRequest);
        }

        private Task<byte[]> Dispatch(byte[] serializedRequest)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(InProcEndpoint<I>)}"))
            {
                try
                {
                    return _endpointPipeLine.ServicePipeLine<I>(serializedRequest, _binding?.Pipeline, activity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }
    }
}
