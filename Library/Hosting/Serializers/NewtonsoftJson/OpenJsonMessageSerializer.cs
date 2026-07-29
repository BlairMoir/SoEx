using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoEx.Abstractions;

namespace SoEx.Hosting.Serializers.NewtonsoftJson
{
    public class OpenJsonMessageSerializer : IMessageSerializer
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            //SerializationBinder = new MethodSerializationBinder()
        };

        public T? Deserialize<T>(byte[] payload)
        {
            var utf8String = Encoding.UTF8.GetString(payload);
            return JsonConvert.DeserializeObject<T>(utf8String, jsonSerializerSettings);
        }

        public byte[] Serialize<T>(T? @object)
        {
            var utf8String = JsonConvert.SerializeObject(@object, jsonSerializerSettings);
            return Encoding.UTF8.GetBytes(utf8String);
        }
    }
}
