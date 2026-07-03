
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
        private WebApplication? _listener;
        private readonly GrpcDispatchProvider _grpcProvider;

        public GrpcEndpoint(ILogger<GrpcEndpoint<I>> logger, IEndpointPipeline endpointPipeLine)
        {
            _logger = logger;
            _endpointPipeLine = endpointPipeLine;
            _grpcProvider = new GrpcDispatchProvider(DispatchAsync);

        }

        public void Bind(Binding binding, string componentName)
        {
            if (binding is GrpcBinding<I> grpcBinding)
            {
                _grpcBinding= grpcBinding;
            }
        }


        public async Task Listen()
        {
            ArgumentNullException.ThrowIfNull(_grpcBinding, "Binding must be set before listening");

            var builder = WebApplication.CreateSlimBuilder();
            builder.WebHost.ConfigureKestrel(ConfigureKestrel);
            builder.Services.AddGrpc();
            builder.Services.AddSingleton<IServiceMethodProvider<GrpcEndpointService>>( f=> _grpcProvider );
            _listener = builder.Build();
            _listener.MapGrpcService<GrpcEndpointService>();
            await _listener.StartAsync();

        }

        private void ConfigureKestrel(KestrelServerOptions k)
        {
            if (_grpcBinding is null)
                throw new ArgumentNullException(nameof(_grpcBinding));

            if (string.IsNullOrEmpty(_grpcBinding.Config.BindAddress)
                || _grpcBinding.Config.BindAddress is "*" or "+" or "0.0.0.0")
            {
                k.ListenAnyIP(_grpcBinding.Config.Port,ConfigureOptions);
            }
            else
            {
                k.Listen( IPAddress.Parse(_grpcBinding.Config.BindAddress), _grpcBinding.Config.Port, ConfigureOptions);
            }
        }

        private void ConfigureOptions(ListenOptions o)
        {
            if (_grpcBinding is null)
                throw new ArgumentNullException(nameof(_grpcBinding));

            var protection = _grpcBinding.Config.Protection;

            o.Protocols = HttpProtocols.Http2;
            if (protection is ClearTextGrpc)
                return;

            if (protection is GrpcCertificate certificate)
            {
                o.UseHttps(certificate.Certificate);
                return;
            }

            if (protection is GrpcCertificateFromPath certificateFromPath)
            {
                o.UseHttps(certificateFromPath.CertPath, certificateFromPath.CertPassword);
                return;
            }

            throw new NotSupportedException($"Unsupported protection {protection}");
        }

        public async Task Close()
        {
            if (_listener is not null)
            {
                await _listener.StopAsync();
                await _listener.DisposeAsync();
            }
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

        internal sealed class GrpcEndpointService
        {
        }

        private sealed class GrpcDispatchProvider : IServiceMethodProvider<GrpcEndpointService>
        {
            private readonly Func<byte[], Task<byte[]>> _dispatchHandler;

            public GrpcDispatchProvider(Func<byte[],Task<byte[]>> dispatchHandler)
            {
                _dispatchHandler = dispatchHandler;
            }

            public void OnServiceMethodDiscovery(ServiceMethodProviderContext<GrpcEndpointService> ctx) =>
                ctx.AddUnaryMethod<byte[], byte[]>(
                    GrpcInvoke.Descriptor,
                    Array.Empty<object>(),
                    (service, request, callCtx) => _dispatchHandler(request)
                );
        }
    }
}
