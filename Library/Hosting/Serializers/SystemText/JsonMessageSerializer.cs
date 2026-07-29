using System.Text.Json;
using SoEx.Abstractions;
using SoEx.Hosting.Serializers.Common;

namespace SoEx.Hosting.Serializers.SystemText
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonMessageSerializer(Type[] knownTypes)
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new KnownTypeObjectConverter(new KnownTypeRegistry(knownTypes)));
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
