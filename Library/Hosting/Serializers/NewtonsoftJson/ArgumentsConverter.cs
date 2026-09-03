using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoEx.Hosting.Serializers.NewtonsoftJson;

public class ArgumentsConverter : JsonConverter
{
    private Type[] _declaredTypes;

    public ArgumentsConverter(Type[] declaredTypes)
    {
        _declaredTypes = declaredTypes;
    }

    private Type TargetOf(int argument)
    {
        if (argument < _declaredTypes.Length)
        {
            return _declaredTypes[argument];
        }
        return typeof(object);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var values = (object?[]?)value;
        if (values == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartArray();

        for (int argument = 0; argument < values.Length; argument++)
        {
            object? current = values[argument];
            if (current == null)
            {
                writer.WriteNull();
                continue;
            }
            serializer.Serialize(writer, current,TargetOf(argument));
        }

        writer.WriteEndArray();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if(reader.TokenType == JsonToken.Null)
           return null;

        JArray array = JArray.Load(reader);
        var arguments = new object?[array.Count];
        for (int argument = 0; argument < array.Count; argument++)
        {
            arguments[argument] = array[argument].ToObject(TargetOf(argument),serializer);
        }

        return arguments;
    }

    public override bool CanConvert(Type objectType) => objectType == typeof(object[]);
}
