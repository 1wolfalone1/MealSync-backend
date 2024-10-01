namespace MealSync.API.Shared;

public static class Endpoints
{
    public const string BASE = "/api/v1";

    // Auth
    public const string LOGIN_USERNAME_PASS = "auth/login";
    public const string ALL_DORMITORY = "dormitory/all";
    public const string GET_BUILDING_BY_DORMITORY = "dormitory/{id}/building";
    public const string SHOP_REGISTER = "auth/shop-register";
    public const string REGISTER_CUSTOMER = "auth/customer-register";

    // Shop Owner
    public const string CREATE_FOOD = "shop-owner/food/create";

    // Option Group
    public const string CREATE_OPTION_GROUP = "shop-owner/option-group/create";
}