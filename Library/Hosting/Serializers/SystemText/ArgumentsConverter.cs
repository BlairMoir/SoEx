using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoEx.Hosting.Serializers.SystemText;

public class ArgumentsConverter : JsonConverter<object?[]>
{
    private Type[] _declaredTypes;

    public ArgumentsConverter(Type[] declaredTypes)
    {
        _declaredTypes = declaredTypes;
    }

    public override object?[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException($@"Expected an array of arguments but found {reader.TokenType}");

        var arguments = new List<object?>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            arguments.Add(JsonSerializer.Deserialize(ref reader, TargetOf(arguments.Count), options));
        }

        return arguments.ToArray();
    }

    private Type TargetOf(int argument)
    {
        if (argument < _declaredTypes.Length)
        {
            return _declaredTypes[argument];
        }
        return typeof(object);
    }

    public override void Write(Utf8JsonWriter writer, object?[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        for(int argument = 0; argument < value.Length; argument++)
        {
            JsonSerializer.Serialize(writer, value[argument], TargetOf(argument), options);
        }
        writer.WriteEndArray();
    }
}
