namespace SoEx.Hosting.Serializers.Common;

public static class KnownCollections
{
    private static readonly Type[] KeyTypes = { typeof(string), typeof(int), typeof(long), typeof(Guid) };

    public static IEnumerable<Type> For(Type[] types)
    {
        foreach (var type in types)
        {
            if(type.FullName is null)
                continue;

            var collectionTypes = For(type);
            foreach (var collectionType in collectionTypes)
            {
                yield return collectionType;
            }
        }
    }

    private static IEnumerable<Type> For(Type type)
    {
        yield return type.MakeArrayType();
        yield return typeof(List<>).MakeGenericType(type);
        yield return typeof(HashSet<>).MakeGenericType(type);
        foreach(var keyType in KeyTypes)
        {
            yield return typeof(Dictionary<,>).MakeGenericType(keyType, type);
        }
    }
}
