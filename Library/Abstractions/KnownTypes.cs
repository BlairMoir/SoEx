using System.Collections.ObjectModel;

namespace SoEx.Abstractions;

public sealed class KnownTypes
{
    private readonly ReadOnlyCollection<Type> _knownTypes;

    public KnownTypes(params Type[] knownTypes)
    {
        var clonedTypes = (Type[])knownTypes.Clone();
        _knownTypes = Array.AsReadOnly(clonedTypes);
    }
    public IReadOnlyCollection<Type> Types => _knownTypes;
}
