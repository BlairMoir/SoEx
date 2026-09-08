using Grpc.AspNetCore.Server.Model;

namespace SoEx.Transport.Grpc;

public sealed class GrpcDispatchProvider<I> : IServiceMethodProvider<GrpcEndpointService>
{
    private readonly Func<byte[], Task<byte[]>> _dispatchHandler;

    public GrpcDispatchProvider(Func<byte[],Task<byte[]>> dispatchHandler)
    {
        _dispatchHandler = dispatchHandler;
    }

    public void OnServiceMethodDiscovery(ServiceMethodProviderContext<GrpcEndpointService> ctx) =>
        ctx.AddUnaryMethod<byte[], byte[]>(
            GrpcInvoke<I>.Descriptor,
            Array.Empty<object>(),
            (service, request, callCtx) => _dispatchHandler(request)
        );
}
