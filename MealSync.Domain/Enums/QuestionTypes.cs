using System.ComponentModel;

namespace MealSync.Domain.Enums;

public enum QuestionTypes
{
    [Description("Radio")]
    Radio = 1,
    [Description("CheckBox")]
    CheckBox = 2,
}