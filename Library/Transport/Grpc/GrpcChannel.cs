using System.Collections.Concurrent;
using System.Diagnostics;
using Grpc.Core;
using SoEx.Abstractions;
using SoEx.Topology;
using GrpcClient = Grpc.Net.Client.GrpcChannel;

namespace SoEx.Transport.Grpc
{
    public class GrpcChannel<I> : IChannel where I : class
    {
        GrpcBinding<I>? _grpcBinding;
        static readonly ConcurrentDictionary<Uri, Lazy<GrpcClient>> s_channels = new();

        public Type Contract => typeof(I);

        public void Bind(Binding binding)
        {
            if (binding is GrpcBinding<I> grpcBinding)
            {
                _grpcBinding = grpcBinding;
            }
        }

        public IPipeline? Pipeline => _grpcBinding?.Pipeline;

        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(GrpcChannel<I>)}"))
            {
                try
                {
                    Debug.Assert(_grpcBinding is not null);

                    using(var call = InvokeGrpcCall(payload))
                    {
                        return await call.ResponseAsync.ConfigureAwait(false);
                    }
                }
                catch
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }

        private AsyncUnaryCall<byte[]> InvokeGrpcCall(byte[] payload)
        {
            var grpcChannel = GrpcClientChannel();
            var callInvoker = grpcChannel.CreateCallInvoker();
            var call = callInvoker.AsyncUnaryCall(GrpcInvoke<I>.Descriptor, host: null, new CallOptions(), payload);
            return call;
        }

        private GrpcClient GrpcClientChannel()
        {
            Debug.Assert(_grpcBinding is not null);
            return s_channels.GetOrAdd(_grpcBinding.Transport.Address, LazyClient).Value;
        }

        private static Lazy<GrpcClient> LazyClient(Uri uri) => new Lazy<GrpcClient>(() => GrpcClient.ForAddress(uri));
    }
}
