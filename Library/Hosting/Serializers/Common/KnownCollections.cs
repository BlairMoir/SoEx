namespace SoEx.Hosting.Serializers.Common
{
    public static class KnownCollections
    {
        private static readonly Type[] KeyTypes = { typeof(string), typeof(int), typeof(long), typeof(Guid) };

        public static IEnumerable<Type> For(IReadOnlyCollection<Type> types)
        {
            foreach (Type type in types)
            {
                if (type.FullName is null)
                    continue;

                IEnumerable<Type> collectionTypes = For(type);
                foreach (Type collectionType in collectionTypes)
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
            foreach (Type keyType in KeyTypes)
            {
                yield return typeof(Dictionary<,>).MakeGenericType(keyType, type);
            }
        }
    }
}
