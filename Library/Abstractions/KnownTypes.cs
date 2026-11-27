using System.Collections.ObjectModel;

namespace SoEx.Abstractions
{
    public sealed class KnownTypes
    {
        private readonly ReadOnlyCollection<Type> _knownTypes;

        public KnownTypes(params Type[] knownTypes)
        {
            ArgumentNullException.ThrowIfNull(knownTypes);

            var uniqueTypes = new List<Type>();
            foreach (Type type in knownTypes)
            {
                ArgumentNullException.ThrowIfNull(type, nameof(knownTypes));
                if (!uniqueTypes.Contains(type))
                {
                    uniqueTypes.Add(type);
                }
            }
            _knownTypes = uniqueTypes.AsReadOnly();
        }
        public IReadOnlyCollection<Type> Types => _knownTypes;
    }
}
