using System.Reflection;
using System.Text.Json;

namespace SoEx.Hosting.Serializers.Common;

public static class ContractMethod
{
    internal static MethodInfo[] Operations(Type contract)
    {
        var operations = new List<MethodInfo>();
        operations.AddRange(contract.GetMethods());

        foreach (Type inherited in contract.GetInterfaces())
        {
            operations.AddRange(inherited.GetMethods());
        }

        return operations.ToArray();
    }

    internal static MethodInfo? Find(Type contract, string methodName)
    {
        MethodInfo[] operations = Operations(contract);
        MethodInfo? operation = operations.FirstOrDefault(o => o.Name == methodName);
        return operation;
    }

    internal static Type[] ParameterTypes(Type contract, string? methodName)
    {
        if (methodName is null)
            return [];

        var method = Find(contract, methodName);
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

        var method = Find(contract, methodName);
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
