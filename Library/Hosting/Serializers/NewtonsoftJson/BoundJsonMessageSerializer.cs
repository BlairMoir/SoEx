using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoEx.Abstractions;
using SoEx.Hosting.Serializers.Common;

namespace SoEx.Hosting.Serializers.NewtonsoftJson
{
    public class BoundJsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public BoundJsonMessageSerializer(KnownTypes knownTypes)
        {
            jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
                SerializationBinder = new SerializationBinder(new KnownTypeRegistry(knownTypes.Types)),
            };
        }

        public T? Deserialize<T>(byte[] payload)
        {
            var utf8String = Encoding.UTF8.GetString(payload);
            return JsonConvert.DeserializeObject<T>(utf8String, jsonSerializerSettings);
        }

        public byte[] Serialize<T>(T? @object)
        {
            var utf8String = JsonConvert.SerializeObject(@object, typeof(T), jsonSerializerSettings);
            return Encoding.UTF8.GetBytes(utf8String);
        }
    }
}
