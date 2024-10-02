using System.ComponentModel;

namespace MealSync.Domain.Enums;

public enum OptionGroupTypes
{
    [Description("Radio")]
    Radio = 1,
    [Description("CheckBox")]
    CheckBox = 2,
}