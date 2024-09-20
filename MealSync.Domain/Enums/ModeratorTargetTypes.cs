using System.ComponentModel;

namespace MealSync.Domain.Enums;

public enum ModeratorTargetTypes
{
    [Description("order")]
    Order = 1,
    [Description("customer")]
    Customer = 2,
    [Description("withdrawal")]
    Withdrawal = 3,
    [Description("report")]
    Report = 4,
    [Description("shop")]
    Shop = 5,
}