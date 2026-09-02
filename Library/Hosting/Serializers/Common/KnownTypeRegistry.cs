using SoEx.Context;

namespace SoEx.Hosting.Serializers.Common
{
    public sealed class KnownTypeRegistry
    {
        private readonly Dictionary<Type, string> _typeLookup = new();
        private readonly Dictionary<string, Type> _discriminatorLookup = new(StringComparer.Ordinal);
        private readonly HashSet<Type> _collections = new();

        public KnownTypeRegistry(IReadOnlyCollection<Type> knownTypes)
        {
            Register(typeof(InvocationContext));
            Register(typeof(EntryContext));
            Register(typeof(PreviousEntryContext));

            foreach (var type in knownTypes)
                Register(type);

            foreach (var type in KnownCollections.For(knownTypes))
                RegisterCollection(type);
        }

        private bool Register(Type type)
        {
            if (type.FullName is null)
                return false;

            string discriminator = Discriminate(type);
            if (!_typeLookup.TryAdd(type, discriminator))
                return false;

            _discriminatorLookup[discriminator] = type;
            return true;
        }

        private void RegisterCollection(Type type)
        {
            if(Register(type))
            {
                _collections.Add(type);
            }
        }

        private static string Discriminate(Type type)
        {
            if (type.IsArray)
                return $"{type.GetElementType()!.FullName}[]";

            if (type.IsGenericType && !type.ContainsGenericParameters)
            {
                var definition = type.GetGenericTypeDefinition();
                var args = type.GetGenericArguments();

                if (definition == typeof(List<>))       return $"List<{args[0].FullName}>";
                if (definition == typeof(HashSet<>))    return $"HashSet<{args[0].FullName}>";
                if (definition == typeof(Dictionary<,>)) return $"Dictionary<{args[0].FullName};{args[1].FullName}>";

                return $"{definition.FullName}<{string.Join(";", args.Select(Discriminate))}>";
            }

            return type.FullName!;
        }

        public bool TryGetDiscriminator(Type type, out string discriminator) =>
            _typeLookup.TryGetValue(type, out discriminator!);

        public bool TryGetType(string discriminator, out Type type) =>
            _discriminatorLookup.TryGetValue(discriminator, out type!);

        public bool IsCollection(Type type) => _collections.Contains(type);
    }
}
