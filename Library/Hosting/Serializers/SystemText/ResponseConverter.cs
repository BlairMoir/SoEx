
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoEx.Hosting.Serializers.SystemText;

public class ResponseConverter : JsonConverter<object?>
{
    private readonly Type _returnType;

    public ResponseConverter(Type returnType)
    {
        _returnType = returnType;
    }

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        return JsonSerializer.Deserialize(ref reader, _returnType, options);
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value, _returnType, options);
    }
}
