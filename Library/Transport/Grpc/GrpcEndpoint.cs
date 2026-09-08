
using System.Diagnostics;
using System.Net;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SoEx.Endpoint;
using SoEx.Topology;
using SoEx.Transport.Grpc.Protection;

namespace SoEx.Transport.Grpc
{
    public class GrpcEndpoint<I> : IEndpoint where I : class
    {
        private readonly ILogger<GrpcEndpoint<I>> _logger;
        private GrpcBinding<I>? _grpcBinding;
        private readonly IEndpointPipeline _endpointPipeLine;
        private readonly GrpcEndpointListener _listener;

        public GrpcEndpoint(ILogger<GrpcEndpoint<I>> logger, IEndpointPipeline endpointPipeLine, GrpcEndpointListener listener)
        {
            _logger = logger;
            _endpointPipeLine = endpointPipeLine;
            _listener = listener;
            IServiceMethodProvider<GrpcEndpointService> provider = new GrpcDispatchProvider<I>(DispatchAsync);
            _listener.RegisterDispatch(provider);
        }

        public void Bind(Binding binding, string componentName)
        {
            if (binding is GrpcBinding<I> grpcBinding)
            {
                _grpcBinding= grpcBinding;
                _listener.Bind(_grpcBinding.Config);
            }
        }

        public async Task Listen()
        {
            await _listener.Listen();
        }

        public async Task Close()
        {
            await _listener.Close();
        }

        private async Task<byte[]> DispatchAsync(byte[] serializedRequest)
        {
            using(var activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(GrpcEndpoint<I>)}", ActivityKind.Server))
            {
                try
                {
                    return await _endpointPipeLine.ServicePipeLine<I>(serializedRequest, _grpcBinding?.Pipeline, activity);
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
