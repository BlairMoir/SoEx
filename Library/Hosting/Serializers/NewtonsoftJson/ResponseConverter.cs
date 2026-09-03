using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoEx.Hosting.Serializers.NewtonsoftJson;

public class ResponseConverter : JsonConverter
{
    private Type? _returnType;

    public ResponseConverter(Type returnType)
    {
        _returnType = returnType;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if(value is null)
        {
            writer.WriteNull();
            return;
        }

        serializer.Serialize(writer, value, _returnType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        return JToken.Load(reader).ToObject(_returnType,serializer);
    }

    public override bool CanConvert(Type objectType) => objectType == typeof(object);
}
