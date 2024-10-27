namespace MealSync.API.Shared;

public static class Endpoints
{
    public const string BASE = "/api/v1";

    // Auth
    public const string LOGIN_USERNAME_PASS = "auth/login";
    public const string SHOP_REGISTER = "auth/shop-register";
    public const string REGISTER_CUSTOMER = "auth/customer-register";

    // Shop Owner
    public const string GET_FOOD = "shop-owner/food";
    public const string CREATE_FOOD = "shop-owner/food/create";
    public const string UPDATE_FOOD = "shop-owner/food/update";
    public const string UPDATE_FOOD_STATUS = "shop-owner/food/{id}/status";
    public const string DELETE_FOOD_STATUS = "shop-owner/food/{id}";
    public const string GET_SHOP_PROFILE = "shop-owner/full-infor";
    public const string UPDATE_SHOP_PROFILE = "shop-owner/profile";
    public const string UPDATE_SHOP_ACTIVE_INACTIVE = "shop-owner/shop-owner/active-inactive";
    public const string GET_SHOP_FOOD_DETAIL = "shop-owner/food/{id}/detail";

    // Shop Owner For Web
    public const string GET_FOOD_FOR_WEB = "web/shop-owner/food";

    // Option Group
    public const string GET_ALL_SHOP_OPTION_GROUP = "shop-owner/option-group";
    public const string CREATE_OPTION_GROUP = "shop-owner/option-group/create";
    public const string UPDATE_OPTION_GROUP = "shop-owner/option-group/{id:long}";
    public const string UPDATE_OPTION_GROUP_STATUS = "shop-owner/option-group/{id:long}/status";
    public const string DELETE_OPTION_GROUP = "shop-owner/option-group/{id:long}";
    public const string GET_DETAIL_OPTION_GROUP = "shop-owner/option-group/{id:long}";
    public const string LINK_FOOD_OPTION_GROUP = "shop-owner/option-group/link-food";
    public const string UNLINK_FOOD_OPTION_GROUP = "shop-owner/option-group/unlink-food";

    // Option
    public const string CREATE_OPTION = "shop-owner/option";

    // Shop Category
    public const string CREATE_SHOP_CATEGORY = "shop-owner/category/create";
    public const string REARRANGE_SHOP_CATEGORY = "shop-owner/category/re-arrange";
    public const string GET_APP_SHOP_CATEGORY = "shop-owner/category";
    public const string UPDATE_APP_SHOP_CATEGORY = "shop-owner/category/{id}";
    public const string DELETE_APP_SHOP_CATEGORY = "shop-owner/category/{id}";
    public const string GET_APP_SHOP_CATEGORY_DETAIL = "shop-owner/category/{id}";
    public const string GET_WEB_SHOP_CATEGORY_DETAIL = "web/shop-owner/category";

    // Shop
    public const string GET_TOP_SHOP = "shop/top";
    public const string FAVOURITE_SHOP = "shop/{id}/favourite";
    public const string GET_FAVOURITE_SHOP = "shop/favourite";
    public const string GET_SHOP_INFO = "shop/{id}/info";
    public const string GET_SHOP_FOOD = "shop/{id}/food";
    public const string GET_ALL_SHOP_FOOD = "shop/{id}/food/all";
    public const string GET_SHOP_PROMOTION = "shop/{id:long}/promotion";
    public const string GET_SHOP_PROMOTION_BY_CONDITION = "shop/{id:long}/promotion/filter";

    // Shop delivery staff
    public const string GET_SHOP_DELIVER_STAFF_AVAILABLE = "shop/shop-delivery-staff/available";

    // Dormitory
    public const string ALL_DORMITORY = "dormitory/all";

    // Building
    public const string GET_BUILDING_BY_DORMITORY = "dormitory/{id}/building";
    public const string CHECK_BUILDING_SELECTION = "customer/building/selected/check";

    // Operating Slot
    public const string ADD_OPERATING_SLOT = "shop-owner/operating-slot";
    public const string UPDATE_OPERATING_SLOT = "shop-owner/operating-slot/{id}";
    public const string DELETE_OPERATING_SLOT = "shop-owner/operating-slot/{id}";
    public const string GET_ALL_OPERATING_SLOT = "shop-owner/operating-slot";

    // Food
    public const string GET_TOP_FOOD = "food/top";
    public const string GET_FOOD_DETAIL = "shop/{shopId}/food/{foodId}";
    public const string GET_FOOD_BY_IDS = "food";

    // Storage
    public const string UPLOAD_FILE = "storage/file/upload";
    public const string DELETE_FILE = "storage/file/delete";

    // Platform category
    public const string GET_ALL_PLATFORM_CATEGORY = "platform-category";

    // Customer Building
    public const string UPDATE_CUSTOMER_BUILDING = "customer/building/update";

    // Customer
    public const string GET_CUSTOMER_INFO = "customer/profile";
    public const string GET_ALL_CUSTOMER_BUILDING = "customer/building";
    public const string UPDATE_AVATAR = "customer/avatar";

    // Order
    public const string CREATE_ORDER = "customer/order";
    public const string CANCEL_ORDER = "customer/order/{id:long}/cancel";
    public const string CREATE_REFUND = "customer/order/refund";
    public const string GET_IPN = "customer/order/IPN";
    public const string GET_ORDER_DETAIL = "customer/order/{id:long}";
    public const string GET_ORDER_HISTORY = "customer/order/history";
    public const string GET_ORDER_FOR_SHOP_BY_STATUS = "shop-owner/order";
    public const string GET_ORDER_DETAIL_FOR_SHOP = "shop-owner/order/{id:long}";
    public const string GET_PAYMENT_STATUS_BY_ORDER_ID = "customer/order/{id:long}/payment/status";
    public const string SHOP_REJECT_ORDER = "shop-owner/order/{id:long}/reject";
    public const string SHOP_CONFIRM_ORDER = "shop-owner/order/{id:long}/confirm";
    public const string SHOP_CANCEL_ORDER = "shop-owner/order/{id:long}/cancel";
    public const string SHOP_PREPARING_ORDER = "shop-owner/order/{id:long}/preparing";

    // Review
    public const string REVIEW_ORDER = "customer/order/review";
    public const string REVIEW_OF_SHOP = "shop/{shopId:long}/review";
    public const string REVIEW_SUMMARY_OF_SHOP = "shop/{shopId:long}/review/overview";
}