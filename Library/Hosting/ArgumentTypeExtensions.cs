using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace SoEx.Hosting;

internal static class ArgumentTypeExtensions
{
    internal static T? ToResponseType<T>(this object? value)
    {
        return (T?)value.ToTargetType(typeof(T));
    }

    internal static object? ToArgumentType(this object? value, Type targetType)
    {
        return ToTargetType(value, targetType);
    }

    private static object? ToTargetType(this object? value, Type targetType)
    {
        if(value is null)
            return null;

        if (targetType.IsInstanceOfType(value))
            return value;

        targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (targetType.IsEnum && value is string stringArgument)
            return Enum.Parse(targetType, stringArgument, ignoreCase: true);

        if (targetType.IsEnum)
            return Enum.ToObject(targetType, value);

        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(value.GetType()))
            return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }

    internal static async Task<object?> TaskResult(this object value)
    {
        if (value is Task task)
        {
            await task;
            var property = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                throw new InvalidOperationException("Task does not have a return value (" + task.GetType().ToString() +
                                                    ")");
            return property.GetValue(task);
        }
        throw new ArgumentException("Value must be a task");
    }
}
