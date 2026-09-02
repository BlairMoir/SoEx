using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SoEx.Abstractions;
using SoEx.Hosting.Serializers.Common;

namespace SoEx.Hosting.Serializers.SystemText
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonMessageSerializer(KnownTypes knownTypes)
        {
            _options = new JsonSerializerOptions();
            _options.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            {
                Modifiers = { info => ApplyPolymorphism(knownTypes, info) },
            };
            _options.Converters.Add(new KnownTypeObjectConverter(new KnownTypeRegistry(knownTypes.Types)));
        }

        private void ApplyPolymorphism(KnownTypes knownTypes, JsonTypeInfo info)
        {
            if (info.Kind != JsonTypeInfoKind.Object)
                return;

            var types = knownTypes.Types.ToArray();
            var derived = types.Where( t=> t != info.Type && info.Type.IsAssignableFrom(t)).ToArray();

            if (derived.Length == 0)
                return;

            info.PolymorphismOptions = new JsonPolymorphismOptions()
            {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = false,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
            };

            foreach (var type in derived)
            {
                info.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(type,type.FullName!));
            }
        }

        public T? Deserialize<T>(byte[] payload)
        {
            return JsonSerializer.Deserialize<T>(payload, _options);
        }

        public byte[] Serialize<T>(T? @object)
        {
            return JsonSerializer.SerializeToUtf8Bytes(@object, _options);
        }
    }
}
