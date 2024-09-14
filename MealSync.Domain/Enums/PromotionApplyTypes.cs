using System.ComponentModel;

namespace MealSync.Domain.Enums;

public enum PromotionApplyTypes
{
    [Description("Percent")]
    Percent = 1,
    [Description("Absolute")]
    Absolute = 2,
}