using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoEx.Abstractions;

namespace SoEx.Hosting.Serializers.NewtonsoftJson;

public class ArgumentContractResolver : DefaultContractResolver
{
    Type[] _declaredTypes;

    public ArgumentContractResolver(Type[] declaredTypes)
    {
        _declaredTypes = declaredTypes;
        NamingStrategy = new CamelCaseNamingStrategy();
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        if (member.DeclaringType == typeof(InvocationRequest) && member.Name == nameof(InvocationRequest.Arguments))
        {
            property.Converter = new ArgumentsConverter(_declaredTypes);
        }

        return property;
    }
}
