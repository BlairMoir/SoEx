using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace SoEx.Hosting.Serializers.InProcReference
{
    internal static class Immutability<TImmutable> where TImmutable : Attribute
    {
        private static readonly ConcurrentDictionary<Type, bool> s_checked = new();

        private static readonly HashSet<Type> s_builtin =
        [
            typeof(string), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset),
            typeof(TimeSpan), typeof(DateOnly), typeof(TimeOnly), typeof(Guid)
        ];

        public static object? Require(object? obj, string? operation)
        {
            if (obj is null)
                return null;

            Type type = obj.GetType();
            if (!s_checked.GetOrAdd(type, IsImmutable))
            {
                string detailedMessage = ExceptionMessage(type, operation);
                throw new NotSupportedException(detailedMessage);
            }

            return obj;
        }

        public static object?[] Require(object?[] obj, string? operation)
        {
            foreach (object? o in obj)
            {
                Require(o, operation);
            }

            return (object?[])obj.Clone();
        }

        private static bool IsImmutable(Type type)
        {
            if (type.IsPrimitive || type.IsEnum || s_builtin.Contains(type))
                return true;

            if (type.IsDefined(typeof(TImmutable), false))
                return true;

            if (type.IsGenericType && !type.IsNested && type.Assembly == typeof(ImmutableArray).Assembly)
            {
                foreach (Type argument in type.GetGenericArguments())
                {
                    if (!IsImmutable(argument))
                        return false;
                }
                return true;
            }

            return false;
        }

        private static string ExceptionMessage(Type type, string? operation)
        {
            string message = $"{type.FullName} is not marked [{typeof(TImmutable).Name}].";
            if (operation is not null)
            {
                message = $"{message} Operation: {operation}";
            }
            return message;
        }

    }
}
