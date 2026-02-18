// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace SoEx.Hosting.Serializers.DataContract;


public class ContractLookup
{
    private Dictionary<Type, NameAndNamespace> fromType = [];
    private Dictionary<string, Dictionary<string, Type>> toType = [];

    public ContractLookup() { }
    public ContractLookup(params Type[] types)
    {
        RegisterTypes(types);
    }

    public void RegisterTypes(params Type[] types)
    {
        foreach (var type in types)
        {
            if (type.Namespace is null)
                continue;

            if (!fromType.ContainsKey(type))
            {
                fromType.Add(type, new NameAndNamespace() { Name = type.Name, Namespace = type.Namespace });
            }

            if (!toType.ContainsKey(type.Namespace))
            {
                toType.Add(type.Namespace, new Dictionary<string, Type>());
            }

            if (!toType[type.Namespace].ContainsKey(type.Name))
            {
                toType[type.Namespace].Add(type.Name, type);
            }
        }
    }

    public bool Contains(string typeName, string typeNamespace)
    {
        return toType.ContainsKey(typeNamespace) && toType[typeNamespace].ContainsKey(typeName);
    }

    public bool Contains(Type type)
    {
        return fromType.ContainsKey(type);
    }

    public Type Convert(string typeName, string typeNamespace)
    {
        return toType[typeNamespace][typeName];
    }

    public NameAndNamespace Convert(Type type)
    {
        var t = fromType[type];
        return t;
    }
}

