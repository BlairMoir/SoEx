using System.Collections.Concurrent;
using System.Diagnostics;
using SoEx.Abstractions;

namespace SoEx.Hosting.Serializers.InProcReference
{
    public class InProcReferenceSerializer<TImmutable> : IMessageSerializer where TImmutable : Attribute
    {

        public T? Deserialize<T>(byte[] payload)
        {
            return InProcReferences.Take<T>(payload);
        }

        public T? Deserialize<T>(byte[] payload, Type contractType, string? methodName = null)
        {
            return Deserialize<T>(payload);
        }

        public byte[] Serialize<T>(T? @object)
        {
            object? freeze = Freeze(@object, null);
            byte[] bytes = InProcReferences.Put(freeze);
            return bytes;
        }

        public byte[] Serialize<T>(T? @object, Type contractType, string methodName)
        {
            object? freeze = Freeze(@object, $"{contractType.FullName}.{methodName}");
            byte[] bytes = InProcReferences.Put(freeze);
            return bytes;
        }

        static object? Freeze(object? @object, string? operation) => @object switch
        {
            InvocationRequest r => new InvocationRequest()
            {
                ActivityId = r.ActivityId,
                HasResult = r.HasResult,
                MethodName = r.MethodName,
                Arguments = Immutability<TImmutable>.Require(r.Arguments, operation),
                AmbientContext = (byte[]?)r.AmbientContext?.Clone(),
                FrameworkContext = (byte[]?)r.FrameworkContext?.Clone(),
            },
            InvocationResponse r => new InvocationResponse()
            {
                Response = Immutability<TImmutable>.Require(r.Response, operation),
                AmbientContext = (byte[]?)r.AmbientContext?.Clone(),
            },
            _ => @object
        };

    }
}
