using System.ComponentModel;

namespace MealSync.Domain.Enums;

public enum Roles
{
    [Description("Customer")]
    Customer = 1,
    [Description("ShopOwner")]
    ShopOwner = 2,
    [Description("ShopDelivery")]
    ShopDelivery = 3,
    [Description("Moderator")]
    Moderator = 4,
    [Description("Admin")]
    Admin = 5
}