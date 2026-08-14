namespace SoEx.Abstractions;

public sealed class KnownTypes
{
    private readonly Type[] _knownTypes;

    public KnownTypes(params Type[] knownTypes)
    {
        _knownTypes = knownTypes;
    }
    public IReadOnlyCollection<Type> Types => _knownTypes;
}
