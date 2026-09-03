using SoEx.Context;
using SoEx.Hosting.Serializers.Common;

namespace SoEx.Hosting.Serializers.DataContract;


public class ContractLookup
{
    private readonly Dictionary<Type, NameAndNamespace> _fromType = new();
    private readonly Dictionary<string, Dictionary<string, Type>> _toType = new();

    private const string CollectionNamespace = "urn:soex:knowntypes";

    public ContractLookup(IReadOnlyCollection<Type> knownTypes)
    {
        Register(typeof(InvocationContext),DtoName(typeof(InvocationContext)));
        Register(typeof(EntryContext), DtoName(typeof(EntryContext)));
        Register(typeof(PreviousEntryContext), DtoName(typeof(PreviousEntryContext)));

        foreach (var type in knownTypes)
            Register(type, DtoName(type));

        foreach (var type in KnownCollections.For(knownTypes))
            Register(type, CollectionName(type));
    }

    private void Register(Type type, NameAndNamespace? name)
    {
        if (name is null || !_fromType.TryAdd(type, name))
            return;

        if (!_toType.TryGetValue(name.Namespace, out var inner))
            _toType[name.Namespace] = inner = new();

        inner.TryAdd(name.Name, type);
    }


    private static NameAndNamespace? DtoName(Type type) =>
        type.FullName is null ? null : new NameAndNamespace { Name = XmlName(type), Namespace = type.Namespace! };


    private static string XmlName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var definition = type.Name[..type.Name.IndexOf('`')];
        return definition + "Of" + string.Concat(type.GetGenericArguments().Select(XmlName));
    }

    private static NameAndNamespace CollectionName(Type type)
    {
        var args = type.IsArray ? new[] { type.GetElementType()! } : type.GetGenericArguments();
        var element = string.Join("__", args.Select(a => a.FullName));

        var prefix = type.IsArray ? "Array"
                   : type.GetGenericTypeDefinition() == typeof(List<>) ? "List"
                   : type.GetGenericTypeDefinition() == typeof(HashSet<>) ? "HashSet"
                   : "Dictionary";

        return new NameAndNamespace { Name = $"{prefix}__{element}", Namespace = CollectionNamespace };
    }

    public bool Contains(string name, string ns) => _toType.TryGetValue(ns, out var inner) && inner.ContainsKey(name);
    public bool Contains(Type type) => _fromType.ContainsKey(type);
    public Type Convert(string name, string ns) => _toType[ns][name];
    public NameAndNamespace Convert(Type type) => _fromType[type];
}

