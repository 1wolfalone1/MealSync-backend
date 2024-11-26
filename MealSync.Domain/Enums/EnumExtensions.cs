using System.ComponentModel;

namespace MealSync.Domain.Enums;

public static class EnumExtensions
{
    public static string GetDescription(this System.Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute.Description;
    }

    public static List<string> GetAllDescriptions<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => e.GetDescription())
            .Where(desc => desc != null)
            .ToList();
    }
}
