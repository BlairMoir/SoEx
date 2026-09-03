using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json;
using SoEx.Abstractions;
using SoEx.Hosting.Serializers.Common;

namespace SoEx.Hosting.Serializers.NewtonsoftJson
{
    public class BoundJsonMessageSerializer : IMessageSerializer
    {
        private readonly Type[] _knownTypes;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly ConcurrentDictionary<(Type contract, string method),JsonSerializerSettings> _settingsCache = new();

        public BoundJsonMessageSerializer(KnownTypes knownTypes)
        {
            _knownTypes = knownTypes.Types.ToArray();
            jsonSerializerSettings = Build(_knownTypes,[]);
        }

        private JsonSerializerSettings Build(Type[] knownTypes, Type[] declaredTypes)
        {
            return new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                ContractResolver = new ArgumentContractResolver(declaredTypes),
                SerializationBinder = new SerializationBinder(new KnownTypeRegistry(knownTypes)),
            };
        }

        private JsonSerializerSettings SettingsFor(Type contract, string? methodName)
        {
            if(methodName is null)
                return jsonSerializerSettings;

            Type[] declared = ContractMethod.ParameterTypes(contract, methodName);

            if (declared.Length == 0)
                return jsonSerializerSettings;

            var options = _settingsCache.GetOrAdd((contract, methodName), _ => Build(_knownTypes, declared));
            return options;
        }

        public T? Deserialize<T>(byte[] payload)
        {
            var utf8String = Encoding.UTF8.GetString(payload);
            return JsonConvert.DeserializeObject<T>(utf8String, jsonSerializerSettings);
        }

        public T? Deserialize<T>(byte[] payload, Type contractType, string? methodName)
        {
            var resolvedMethodName =  methodName ?? ContractMethod.PeekMethodName(payload);
            var settings = SettingsFor(contractType, resolvedMethodName);
            var utf8String = Encoding.UTF8.GetString(payload);
            return JsonConvert.DeserializeObject<T>(utf8String, settings);
        }

        public byte[] Serialize<T>(T? @object)
        {
            var utf8String = JsonConvert.SerializeObject(@object, typeof(T), jsonSerializerSettings);
            return Encoding.UTF8.GetBytes(utf8String);
        }

        public byte[] Serialize<T>(T? @object, Type contractType, string methodName)
        {
            var settings = SettingsFor(contractType, methodName);
            var utf8String = JsonConvert.SerializeObject(@object, typeof(T), settings);
            return Encoding.UTF8.GetBytes(utf8String);
        }
    }
}
