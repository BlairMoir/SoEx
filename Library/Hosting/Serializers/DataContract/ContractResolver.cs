using System.Runtime.Serialization;
using System.Xml;

namespace SoEx.Hosting.Serializers.DataContract;

public class ContractResolver : DataContractResolver
{
    readonly ContractLookup _contractLookup;

    public ContractResolver(ContractLookup contractLookup)
    {
        _contractLookup = contractLookup;
    }

    public override Type? ResolveName(string typeName, string? typeNamespace, Type? declaredType, DataContractResolver knownTypeResolver)
    {
        if (typeNamespace is not null && _contractLookup.Contains(typeName, typeNamespace))
        {
            return _contractLookup.Convert(typeName, typeNamespace);
        }
        return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null!);
    }
    public override bool TryResolveType(Type type, Type? declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString? typeName, out XmlDictionaryString? typeNamespace)
    {
        if (_contractLookup.Contains(type))
        {
            NameAndNamespace nameAndNamespace = _contractLookup.Convert(type);
            typeName = new XmlDictionaryString(XmlDictionary.Empty, nameAndNamespace.Name, 0);
            typeNamespace = new XmlDictionaryString(XmlDictionary.Empty, nameAndNamespace.Namespace, 0);
            return true;
        }

        return knownTypeResolver.TryResolveType(type, declaredType, null!, out typeName, out typeNamespace);
    }
}

