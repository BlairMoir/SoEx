using System.Collections.Concurrent;
using System.Diagnostics;
using Grpc.Core;
using Grpc.Net.Client;
using SoEx.Topology;
using GrpcClient = Grpc.Net.Client.GrpcChannel;

namespace SoEx.Transport.Grpc
{
    public class GrpcChannel<I> : IChannel where I : class
    {
        GrpcBinding<I>? _grpcBinding;
        static readonly ConcurrentDictionary<Uri, Lazy<GrpcClient>> s_channels = new();
        static readonly ConcurrentDictionary<Uri, long> s_lastFailure = new();
        private const long FailureMemoryMillis = 45000;


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

                    // sequential failover if a backend is down
                    long now = Environment.TickCount64;
                    Uri[] uris = _grpcBinding.Transport.Address.Uris
                        .OrderBy( u => HasFailures(u, now)).ToArray();
                    foreach (var uri in uris[..^1])
                    {
                        try
                        {
                            return await InvokeAsync(payload, uri, activity).ConfigureAwait(false);
                        }
                        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                        {
                            // continue if the backend is unreachable
                        }
                    }
                    return await InvokeAsync(payload, uris[^1],activity).ConfigureAwait(false);
                }
                catch
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }

        static bool HasFailures(Uri uri, long now)
        {
            if (!s_lastFailure.TryGetValue(uri, out var failedAt))
                return false;

            if (now - failedAt < FailureMemoryMillis)
                return true;

            return !s_lastFailure.TryUpdate(uri, now, failedAt);
        }

        private async Task<byte[]> InvokeAsync(byte[] payload, Uri uri, Activity? activity)
        {
            activity?.SetTag("server.address", uri.ToString());
            try
            {
                using (var call = InvokeGrpcCall(uri, payload))
                {
                    var result = await call.ResponseAsync.ConfigureAwait(false);
                    ClearFailure(uri);
                    return result;
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
            {
                s_lastFailure[uri] = Environment.TickCount64;
                throw;
            }
            catch (RpcException)
            {
                ClearFailure(uri);
                throw;
            }
        }

        private static void ClearFailure(Uri uri)
        {
            if(s_lastFailure.ContainsKey(uri))
                s_lastFailure.TryRemove(uri, out _);
        }

        private AsyncUnaryCall<byte[]> InvokeGrpcCall(Uri uri, byte[] payload)
        {
            var grpcChannel = GrpcClientChannel(uri);
            var callInvoker = grpcChannel.CreateCallInvoker();
            var call = callInvoker.AsyncUnaryCall(GrpcInvoke<I>.Descriptor, host: null, new CallOptions(), payload);
            return call;
        }

        private GrpcClient GrpcClientChannel(Uri uri)
        {
            return s_channels.GetOrAdd(uri, LazyClient).Value;
        }

        private static Lazy<GrpcClient> LazyClient(Uri uri) => new Lazy<GrpcClient>(() =>
            GrpcClient.ForAddress(uri, new GrpcChannelOptions()
            {
                HttpHandler  = new SocketsHttpHandler()
                {
                    ConnectTimeout = TimeSpan.FromSeconds(2)
                }
            }));
    }
}
