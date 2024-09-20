namespace MealSync.Domain.Enums;

public enum LoginContextType
{
    AppForUser = 1,

    AppForShopOwnerOrDelivery = 2,

    WebForShop = 3,

    WebForAdminOrModerator = 4
}