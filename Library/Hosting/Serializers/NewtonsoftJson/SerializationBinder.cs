using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoEx.Hosting.Serializers.Common;

namespace SoEx.Hosting.Serializers.NewtonsoftJson
{
    internal sealed class SerializationBinder : ISerializationBinder
    {
        private readonly KnownTypeRegistry _registry;

        public SerializationBinder(KnownTypeRegistry registry)
        {
            _registry = registry;
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            assemblyName = null;
            if (_registry.TryGetDiscriminator(serializedType, out var discriminator))
            {
                typeName = discriminator;
                return;
            }

            throw new JsonSerializationException(
                $"Type '{serializedType.FullName}' is not in KnownTypes");
        }

        public Type BindToType(string? assemblyName, string typeName)
        {
            if (_registry.TryGetType(typeName, out var type))
                return type;

            throw new JsonSerializationException(
                $"Type '{typeName}' is not in KnownTypes");
        }
    }
}
