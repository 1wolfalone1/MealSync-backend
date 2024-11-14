using System.ComponentModel;
using System.Reflection;

namespace MealSync.Application.Common.Utils;

public static class EnumHelper
{
    public static string GetEnumValue<TEnum>(int value) where TEnum : Enum
    {
        var type = typeof(TEnum);
        var name = Enum.GetName(type, value);
        if (name == null)
        {
            return null;
        }

        var field = type.GetField(name);
        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? name;
    }

    public static TEnum? GetEnumByDescription<TEnum>(string description) where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .FirstOrDefault(enumValue =>
                enumValue.GetDescription().Equals(description, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;
        return attribute?.Description ?? value.ToString();
    }

    public static TEnum? GetEnumByValue<TEnum>(int value) where TEnum : struct, Enum
    {
        if (Enum.IsDefined(typeof(TEnum), value))
        {
            return (TEnum)(object)value;
        }

        return null;
    }
}