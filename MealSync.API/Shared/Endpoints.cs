namespace MealSync.API.Shared;

public static class Endpoints
{
    public const string BASE = "/api/v1";

    // Auth
    public const string LOGIN_USERNAME_PASS = "auth/login";
    public const string SHOP_REGISTER = "auth/shop-register";
    public const string REGISTER_CUSTOMER = "auth/customer-register";

    // Shop Owner
    public const string CREATE_FOOD = "shop-owner/food/create";
    public const string GET_SHOP_PROFILE = "shop-owner/profile";
    public const string UPDATE_SHOP_PROFILE = "shop-owner/profile";

    // Option Group
    public const string CREATE_OPTION_GROUP = "shop-owner/option-group/create";

    // Shop Category
    public const string CREATE_SHOP_CATEGORY = "shop-owner/category/create";

    public const string REARRANGE_SHOP_CATEGORY = "shop-owner/category/re-arrange";

    // Shop
    public const string GET_TOP_SHOP = "shop/top";
    public const string FAVOURITE_SHOP = "shop/{id}/favourite";

    // Dormitory
    public const string ALL_DORMITORY = "dormitory/all";

    // Building
    public const string GET_BUILDING_BY_DORMITORY = "dormitory/{id}/building";
    public const string CHECK_BUILDING_SELECTION = "customer/building/selected/check";

    // Operating Slot
    public const string ADD_OPERATING_SLOT = "shop-owner/operating-slot";
    public const string UPDATE_OPERATING_SLOT = "shop-owner/operating-slot/{id}";
    public const string DELETE_OPERATING_SLOT = "shop-owner/operating-slot/{id}";

    // Food
    public const string GET_TOP_FOOD = "food/top";

    // Storage
    public const string UPLOAD_FILE = "storage/file/upload";
    public const string DELETE_FILE = "storage/file/delete";
}