using Grpc.Core;

namespace SoEx.Transport.Grpc;

public static class GrpcInvoke<I>
{
    static readonly Marshaller<byte[]> Marshaller = Marshallers.Create(b => b, b => b);
    public static readonly Method<byte[], byte[]> Descriptor =
        new(MethodType.Unary, serviceName: typeof(I).FullName ?? typeof(I).Name, name: "Invoke", Marshaller, Marshaller);
}
