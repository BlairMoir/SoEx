using System.Text.Json;
using System.Text.Json.Serialization;
using SoEx.Hosting.Serializers.Common;

namespace SoEx.Hosting.Serializers.SystemText;

public class KnownTypeObjectConverter : JsonConverter<object>
{
    KnownTypeRegistry _registry;

    public KnownTypeObjectConverter(KnownTypeRegistry registry)
    {
        _registry = registry;
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(object);

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("_$type", out var typeProperty))
        {
            var discriminator = typeProperty.GetString();
            if (discriminator is not null && _registry.TryGetType(discriminator, out var type))
            {
                if (_registry.IsCollection(type))
                    return JsonSerializer.Deserialize(root.GetProperty("$values").GetRawText(), type, options);

                if (root.TryGetProperty("$value", out var wrappedValue))
                    return JsonSerializer.Deserialize(wrappedValue.GetRawText(), type, options);

                return JsonSerializer.Deserialize(root.GetRawText(), type, options);
            }

            throw new JsonException($"Discriminator '{discriminator}' is not in KnownTypes.");
        }
        return root.ValueKind switch
        {
            JsonValueKind.Number => root.TryGetInt64(out var l) ? (object)l : root.GetDouble(),
            JsonValueKind.String => root.GetString(),
            JsonValueKind.True or JsonValueKind.False => root.GetBoolean(),
            JsonValueKind.Null => null,
            _ => root.Clone()
        };
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        var runtimeType = value.GetType();

        if (runtimeType == typeof(object))
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
            return;
        }

        if (runtimeType.IsPrimitive || value is string || value is decimal)
        {
            JsonSerializer.Serialize(writer, value, runtimeType, options);
            return;
        }

        if (!_registry.TryGetDiscriminator(runtimeType, out var discriminator))
            throw new JsonException($"Type '{runtimeType.FullName}' is not in KnownTypes.");

        writer.WriteStartObject();
        writer.WriteString("_$type", discriminator);

        if (_registry.IsCollection(runtimeType))
        {
            writer.WritePropertyName("$values");
            JsonSerializer.Serialize(writer, value, runtimeType, options);
        }
        else
        {
            using var inner = JsonSerializer.SerializeToDocument(value, runtimeType, options);
            if (inner.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in inner.RootElement.EnumerateObject())
                    property.WriteTo(writer);
            }
            else
            {
                writer.WritePropertyName("$value");
                inner.RootElement.WriteTo(writer);
            }
        }
        writer.WriteEndObject();
    }
}
