using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoEx.Abstractions;

namespace SoEx.Hosting.Serializers.NewtonsoftJson;

public class OperationContractResolver : DefaultContractResolver
{
    Type[] _declaredTypes;
    Type? _returnType;

    public OperationContractResolver(Type[] declaredTypes, Type? returnType)
    {
        _declaredTypes = declaredTypes;
        _returnType = returnType;
        NamingStrategy = new CamelCaseNamingStrategy();
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        if (member.DeclaringType == typeof(InvocationRequest) && member.Name == nameof(InvocationRequest.Arguments))
        {
            property.Converter = new ArgumentsConverter(_declaredTypes);
        }

        if ( _returnType != null && member.DeclaringType == typeof(InvocationResponse) &&
            member.Name == nameof(InvocationResponse.Response))
        {
            property.Converter = new ResponseConverter(_returnType);
        }

        return property;
    }
}
