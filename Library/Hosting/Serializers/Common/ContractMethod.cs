using System.Text.Json;

namespace SoEx.Hosting.Serializers.Common;

public static class ContractMethod
{
    internal static Type[] ParameterTypes(Type contract, string? methodName)
    {
        if (methodName is null)
            return [];

        var method = contract.GetMethod(methodName);
        if (method is null)
            return [];

        var parameterTypes = method.GetParameters().Select(s => s.ParameterType).ToArray();
        return parameterTypes;
    }

    internal static string? PeekMethodName(ReadOnlySpan<byte> payload)
    {
        try
        {
            var reader = new Utf8JsonReader(payload);

            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                return null;

            while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
            {
                if (IsMethodNameProperty(ref reader))
                {
                    if (reader.Read() && reader.TokenType == JsonTokenType.String)
                    {
                        return reader.GetString();
                    }
                    break;
                }
                reader.Skip();
            }
        }
        catch (JsonException)
        {
        }
        return null;
    }

    private static bool IsMethodNameProperty(ref Utf8JsonReader reader)
    {
        return reader.ValueTextEquals("methodName"u8)
               || reader.ValueTextEquals("MethodName"u8);
    }

    internal static Type? ReturnType(Type contract, string? methodName)
    {
        if (methodName is null)
            return null;

        var method = contract.GetMethod(methodName);
        if (method is null)
            return null;

        Type returnType = method.ReturnType;

        if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(Task<>))
            return null;

        Type result = returnType.GetGenericArguments()[0];

        if (result == typeof(object))
            return null;

        return result;
    }
}
