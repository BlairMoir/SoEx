using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.Relay;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Relay
{
    public class RelayEndpoint<I> : IEndpoint where I : class
    {
        readonly ILogger<RelayEndpoint<I>> _logger;
        RelayBinding<I>? _relayBinding;
        IEndpointPipeline _endpointPipeLine;


        public RelayEndpoint(ILogger<RelayEndpoint<I>> logger, IEndpointPipeline endpointPipeLine)
        {
            _logger = logger;
            _endpointPipeLine = endpointPipeLine;

        }

        public void Bind(Binding binding, string componentName)
        {
            if (binding is RelayBinding<I> relayBinding)
            {
                _relayBinding = relayBinding;
            }
        }

        HybridConnectionListener? _listener;
        public async Task Listen()
        {
            if (_relayBinding != null)
            {
                _listener = new HybridConnectionListener(_relayBinding.Transport.Address, _relayBinding.CreateTokenProvider());
                _listener.Connecting += (o, e) => { _logger.LogInformation("Connecting"); };
                _listener.Offline += (o, e) => { _logger.LogInformation("Offline"); };
                _listener.Online += (o, e) => { _logger.LogInformation("Online"); };
                _listener.RequestHandler = RequestHandler;
                await _listener.OpenAsync();
            }
        }

        public async Task Close()
        {
            if (_listener is not null)
            {
                await _listener.CloseAsync();
            }
        }

        public void RequestHandler(RelayedHttpListenerContext context)
        {
            using (var requestMemoryStream = new MemoryStream())
            {
                context.Request.InputStream.CopyTo(requestMemoryStream);
                byte[] seralizedRequest = requestMemoryStream.ToArray();
                using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(RelayEndpoint<I>)}", ActivityKind.Server))
                {
                    try
                    {
                        context.Response.StatusCode = HttpStatusCode.OK;
                        context.Response.StatusDescription = "OK";
                        var serialisedResponse = _endpointPipeLine.ServicePipeLine<I>(seralizedRequest, _relayBinding?.Pipeline, activity).Result;
                        using (var responseMemoryStream = new MemoryStream(serialisedResponse))
                        {
                            responseMemoryStream.CopyTo(context.Response.OutputStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        context.Response.StatusCode = HttpStatusCode.ServiceUnavailable;
                        context.Response.StatusDescription = "Error";
                        activity?.SetStatus(ActivityStatusCode.Error);
                        throw;
                    }
                }
            }
            // The context MUST be closed here
            context.Response.Close();
        }
    }
}
