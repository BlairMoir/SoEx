using System.Collections.Concurrent;
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
        private readonly Type[] _knownTypes;
        private readonly ConcurrentDictionary<(Type contract, string method),JsonSerializerOptions> _optionCache = new();

        public JsonMessageSerializer(KnownTypes knownTypes)
        {
            _knownTypes = knownTypes.Types.ToArray();
            _options = Build(_knownTypes, []);
        }

        private JsonSerializerOptions Build(Type[] knownTypes, Type[] declaredTypes)
        {
            var options = new JsonSerializerOptions();
            var resolver = new DefaultJsonTypeInfoResolver();
            resolver.Modifiers.Add(info => ApplyPolymorphism(knownTypes, info));

            if(declaredTypes.Length > 0)
                resolver.Modifiers.Add(info => ApplyArgumentBinding(declaredTypes, info));

            options.TypeInfoResolver = resolver;
            options.Converters.Add(new KnownTypeObjectConverter(new KnownTypeRegistry(knownTypes)));

            return options;
        }

        private void ApplyArgumentBinding(Type[] declaredTypes, JsonTypeInfo info)
        {
            if (info.Type != typeof(InvocationRequest))
                return;

            JsonPropertyInfo? arguments = info.Properties.FirstOrDefault(p => p.Name == nameof(InvocationRequest.Arguments));

            if (arguments is not null)
            {
                arguments.CustomConverter = new ArgumentsConverter(declaredTypes);
            }
        }

        private void ApplyPolymorphism(Type[] knownTypes, JsonTypeInfo info)
        {
            if (info.Kind != JsonTypeInfoKind.Object)
                return;

            var derived = knownTypes.Where( t=> t != info.Type && info.Type.IsAssignableFrom(t)).ToArray();

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

        private JsonSerializerOptions OptionsFor(Type contract, string? methodName)
        {
            if(methodName is null)
                return _options;

            Type[] declared = ContractMethod.ParameterTypes(contract, methodName);

            if (declared.Length == 0)
                return _options;

            var options = _optionCache.GetOrAdd((contract, methodName), _ => Build(_knownTypes, declared));
            return options;
        }

        public T? Deserialize<T>(byte[] payload)
        {
            return JsonSerializer.Deserialize<T>(payload, _options);
        }

        public T? Deserialize<T>(byte[] payload, Type contractType, string? methodName)
        {
            var resolvedMethodName =  methodName ?? ContractMethod.PeekMethodName(payload);
            var options = OptionsFor(contractType, resolvedMethodName);
            return JsonSerializer.Deserialize<T>(payload, options);
        }

        public byte[] Serialize<T>(T? @object)
        {
            return JsonSerializer.SerializeToUtf8Bytes(@object, _options);
        }

        public byte[] Serialize<T>(T? @object, Type contractType, string methodName)
        {
            var options = OptionsFor(contractType, methodName);
            return JsonSerializer.SerializeToUtf8Bytes(@object, options);
        }
    }
}
