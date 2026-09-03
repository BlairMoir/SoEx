using System.Reflection;

namespace SoEx.Abstractions
{
    public interface IMessageSerializer
    {
        public T? Deserialize<T>(byte[] payload);
        public T? Deserialize<T>(byte[] payload, Type contractType, string? methodName = null);
        public byte[] Serialize<T>(T? @object);
        public byte[] Serialize<T>(T? @object, Type contractType, string methodName);
    }
}
