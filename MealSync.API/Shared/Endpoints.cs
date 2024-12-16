namespace MealSync.API.Shared;

public static class Endpoints
{
    public const string BASE = "/api/v1";

    // Auth
    public const string LOGIN_USERNAME_PASS = "auth/login";
    public const string SHOP_REGISTER = "auth/shop-register";
    public const string REGISTER_CUSTOMER = "auth/customer-register";
    public const string SEND_VERIFY_CODE = "auth/send-code";
    public const string VERIFY_CODE = "auth/verify-code";
    public const string VALID_TOKEN = "auth/valid-token";
    public const string UPDATE_DEVICE_TOKEN = "auth/device-token";
    public const string LOGIN_GOOGLE = "auth/login-google";
    public const string REGISTER_GOOGLE = "auth/register-google";

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
    public const string GET_SHOP_STATISTICS = "shop-owner/order/statistics";
    public const string GET_SHOP_STATISTICS_SUMMARY = "shop-owner/order/statistics/summary";
    public const string UPDATE_SHOP_IS_ACCEPT_ORDER_NEXT_DAY = "shop-owner/is-accept-order-next-day";
    public const string UPDATE_SHOP_IS_AUTO_CONFIRM = "shop-owner/is-auto-confirm";
    public const string UPDATE_SHOP_IS_AUTO_CONFIRM_CONDITION = "shop-owner/is-auto-confirm-condition";
    public const string SEND_VERIFY_UPDATE_SHOP_EMAIL = "shop-owner-staff/update/email/send-verify";
    public const string VERIFY_OLD_EMAIL = "shop-owner-staff/email/verify";
    public const string UPDATE_SHOP_EMAIL = "shop-owner-staff/email/update";
    public const string UPDATE_SHOP_PASSWORD = "shop-owner-staff/password/update";
    public const string UPDATE_SHOP_BANNER = "shop-owner/banner/update";
    public const string UPDATE_SHOP_LOGO = "shop-owner/logo/update";
    public const string UPDATE_SHOP_AVATAR = "shop-owner-staff/avatar/update";
    public const string GET_SHOP_STAFF_INFO = "shop-staff/info";
    public const string UPDATE_SHOP_STAFF_INFO = "shop-staff/info/update";
    public const string CREATE_DEPOSIT_PAYMENT_URL = "shop-owner/deposit";
    public const string GET_SHOPM_MAX_CARRY_WEIGHT = "shop-owner/max-carry-weight";

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
    public const string SHOP_OPTION_GROUPS_WITH_LINK_FOOD_STATUS = "shop-owner/option-group/food-link-status";

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
    public const string SEARCH_SHOP = "shop/search";
    public const string SHOP_INFO_REORDER = "shop/info/re-order/{id:long}";
    public const string SHOP_CATEGORY = "shop/{id:long}/category";
    public const string SHOP_CART_INFO = "shop/cart";

    // Shop delivery staff
    public const string GET_SHOP_DELIVER_STAFF_AVAILABLE = "shop/shop-delivery-staff/available";
    public const string CREATE_SHOP_DELIVERY_STAFF = "shop-owner/delivery-staff/create";
    public const string GET_SHOP_DELIVER_STAFF_OF_SHOP = "shop-owner/delivery-staff";
    public const string GET_DETAIL_SHOP_DELIVER_STAFF = "shop-owner/delivery-staff/{id:long}";
    public const string UPDATE_INFO_SHOP_DELIVERY_STAFF = "shop-owner/delivery-staff/info";
    public const string UPDATE_STATUS_SHOP_DELIVERY_STAFF = "shop-owner/delivery-staff/status";
    public const string DELETE_SHOP_DELIVERY_STAFF = "shop-owner/delivery-staff/delete";

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
    public const string GET_ALL_OPERATING_SLOT_FOR_SHOP_STAFF = "shop-staff/operating-slot";

    // Food
    public const string GET_TOP_FOOD = "food/top";
    public const string GET_FOOD_DETAIL = "shop/{shopId}/food/{foodId}";
    public const string GET_FOOD_BY_IDS = "food";
    public const string FOOD_LINK_SHOP_CATEGORY = "shop-owner/food/link-shop-category";
    public const string FOOD_LINK_OPTION_GROUPS = "shop-owner/food/link-option-group";

    // Food packing unit
    public const string CREATE_FOOD_PACKING_UNIT = "shop-onwer/food-packing-unit";
    public const string GET_FOOD_PACKING_UNIT = "shop-onwer/food-packing-unit";
    public const string DELETE_FOOD_PACKING_UNIT = "shop-onwer/food-packing-unit/{id:long}";
    public const string UPDATE_FOOD_PACKING_UNIT = "shop-onwer/food-packing-unit/{id:long}";

    public const string ADMIN_CREATE_FOOD_PACKING_UNIT = "admin/food-packing-unit";
    public const string ADMIN_UPDATE_FOOD_PACKING_UNIT = "admin/food-packing-unit/{id:long}";
    public const string ADMIN_FOOD_PACKING_UNIT = "admin/food-packing-unit";
    public const string ADMIN_FOOD_PACKING_UNIT_DETAIL = "admin/food-packing-unit/{id:long}";

    // Storage
    public const string UPLOAD_FILE = "storage/file/upload";
    public const string DELETE_FILE = "storage/file/delete";

    // Platform category
    public const string GET_ALL_PLATFORM_CATEGORY = "platform-category";
    public const string CREATE_PLATFORM_CATEGORY = "admin/platform-category";
    public const string UPDATE_PLATFORM_CATEGORY = "admin/platform-category/{id:long}";
    public const string REARRANGE_PLATFORM_CATEGORY = "admin/platform-category/re-arrange";
    public const string GET_PLATFORM_CATEGORY = "admin/platform-category/get-all";
    public const string GET_DETAIL_PLATFORM_CATEGORY = "admin/platform-category/{id:long}";

    // Customer Building
    public const string UPDATE_CUSTOMER_BUILDING = "customer/building/update";

    // Customer
    public const string GET_CUSTOMER_INFO = "customer/profile";
    public const string GET_ALL_CUSTOMER_BUILDING = "customer/building";
    public const string UPDATE_AVATAR = "customer/avatar";
    public const string UPDATE_CUSTOMER_PROFILE = "customer/profile";

    // Order
    public const string CREATE_ORDER = "customer/order";
    public const string CREATE_ORDER_DUMMY = "customer/order/dummy";
    public const string CANCEL_ORDER = "customer/order/{id:long}/cancel";
    public const string CREATE_REFUND = "customer/order/refund";
    public const string GET_IPN = "customer/order/IPN";
    public const string GET_ORDER_DETAIL = "customer/order/{id:long}";
    public const string GET_ORDER_HISTORY = "customer/order/history";
    public const string GET_ORDER_FOR_SHOP_BY_STATUS = "shop-owner/order";
    public const string GET_ORDER_DETAIL_FOR_SHOP = "shop-owner/order/{id:long}";
    public const string GET_ORDER_DETAIL_FOR_SHOP_AND_STAFF = "shop-owner-staff/order/{id:long}";
    public const string GET_PAYMENT_STATUS_BY_ORDER_ID = "customer/order/{id:long}/payment/status";
    public const string SHOP_REJECT_ORDER = "shop-owner/order/{id:long}/reject";
    public const string SHOP_CONFIRM_ORDER = "shop-owner/order/{id:long}/confirm";
    public const string SHOP_CANCEL_ORDER = "shop-owner/order/{id:long}/cancel";
    public const string SHOP_PREPARING_ORDER = "shop-owner/order/{id:long}/preparing";
    public const string SHOP_ASSIGN_ORDER = "shop-owner/order/{id:long}/assign";
    public const string SHOP_DELIVERED_ORDER = "shop-owner/order/{id:long}/delivered";
    public const string SHOP_STAFF_DELIVERED_ORDER = "shop-owner-staff/order/{id:long}/delivered";
    public const string SHOP_STAFF_DELIVERED_ORDER_BY_PROOF = "shop-owner-staff/order/{id:long}/delivered-by-proof";
    public const string SHOP_DELIVERED_FAIL_ORDER = "shop-owner-staff/order/{id:long}/delivery-fail";
    public const string SHOP_DELIVERING_ORDER = "shop-owner/order/delivering";
    public const string SHOP_STAFF_DELIVERING_ORDER = "shop-owner-staff/order/delivering";
    public const string SHOW_QR_FOR_CONFIRM = "customer/order/{id:long}/qr/received";
    public const string COMPLETED_ORDER = "customer/order/confirm/complete";
    public const string GET_REPAYMENT_LINK = "customer/order/{id:long}/re-payment";
    public const string SHOP_DELIVERED_INFOR_EVIDENCE = "shop-owner/order/{id:long}/delivery-infor";
    public const string SHOP_AND_STAFF_DELIVERED_INFOR_EVIDENCE = "shop-owner-staff/order/{id:long}/delivery-infor";
    public const string RE_ORDER = "customer/re-order/food";
    public const string SHOP_UN_ASSIGN_ORDER = "shop-owner/order/{id:long}/un-assign";


    // Report
    public const string CUSTOMER_REPORT_ORDER = "customer/order/report";
    public const string GET_CUSTOMER_REPORT_ORDER = "customer/order/{id:long}/report";
    public const string GET_REPORT_ORDER_OF_SHOP = "shop-owner/order/report";
    public const string SHOP_REPLY_REPORT_ORDER = "shop-owner/order/report/reply";
    public const string GET_CUSTOMER_REPORT_ORDER_FOR_SHOP = "shop-owner/order/report/{id:long}";
    public const string GET_REPORT_OF_CUSTOMER_FOR_SHOP_WEB = "web/shop-owner/order/report";
    public const string GET_CUSTOMER_REPORT_BY_ORDER = "shop-owner/order/{id:long}/report";

    // Shop order for web
    public const string GET_ORDER_FOR_SHOP_WEB_BY_STATUS = "web/shop-owner/order";

    // Delivery package
    public const string CREATE_DELIVERY_PACKAGE = "shop-owner/delivery-package";
    public const string GET_ALL_DELIVERY_PACKAGE = "shop-owner-staff/delivery-package";
    public const string GET_DELIVERY_PACKAGE = "shop-owner-staff/delivery-package/{id:long}";
    public const string GET_DELIVERY_PACKAGE_GROUP = "shop-owner/delivery-package-group";
    public const string UPDATE_DELIVERY_PACKAGE_GROUP = "shop-owner/delivery-package-group";
    public const string GET_TIME_FRAME_ALL_ORDER_UN_ASSIGN = "shop-owner/delivery-package/time-frame/un-assign";
    public const string SUGGEST_CREATE_ASSIGN_ORDER = "shop-owner/delivery-package/suggest-create";
    public const string GET_ALL_DELIVERY_PACKAGE_GROUP_BY_INTERVAL = "shop-owner/delivery-package-group/interval";
    public const string GET_DELIVERY_PACKAGE_FOR_WEB = "web/shop-owner/delivery-package";
    public const string GET_SHOP_OWN_DELIVERY_PACKAGE_FOR_WEB = "web/shop-owner/delivery-package/own";
    public const string SUGGEST_UPDATE_ASSIGN_ORDER = "shop-owner/delivery-package/suggest-update";
    public const string GET_DELIVERY_PACKAGE_HISTORY = "web/shop-owner/delivery-package/history";
    public const string GET_DELIVERY_PACKAGE_CALCULATE_TIME_SUGGEST = "web/shop-owner/delivery-package/calculate-time-suggest";
    public const string GET_DELIVERY_PACKAGE_CALCULATE_TIME_SUGGEST_SHOP_STAFF = "shop-owner-staff/delivery-package/calculate-time-suggest";

    // Review
    public const string REVIEW_ORDER = "customer/order/review";
    public const string REVIEW_OF_SHOP = "shop/{shopId:long}/review";
    public const string REVIEW_SUMMARY_OF_SHOP = "shop/{shopId:long}/review/overview";
    public const string GET_REVIEW_OF_SHOP_OWNER = "shop-onwer/review";
    public const string CREATE_REVIEW_OF_SHOP_OWNER = "shop-onwer/review";
    public const string GET_REVIEW_BASE_ON_ORDER_ID = "shop-onwer/review/order/{id:long}";
    public const string GET_REVIEW_FOR_SHOP_WEB = "web/shop-onwer/review";

    // Promotion
    public const string CREATE_PROMOTION = "shop-owner/promotion/create";
    public const string UPDATE_PROMOTION_INFO = "shop-owner/promotion/info/update";
    public const string UPDATE_PROMOTION_STATUS = "shop-owner/promotion/status/update";
    public const string GET_SHOP_OWNER_PROMOTION = "shop-owner/promotion";
    public const string GET_SHOP_OWNER_PROMOTION_DETAIL = "shop-owner/promotion/{id:long}/detail";

    // Wallet
    public const string GET_SHOP_WALLET = "shop-owner/wallet/summary";
    public const string WITHDRAWAL_REQUEST_SEND_VERIFY_CODE = "shop-owner/withdrawal/send-verify-code";
    public const string WITHDRAWAL_REQUEST = "shop-owner/withdrawal";
    public const string WITHDRAWAL_REQUEST_HISTORY = "shop-owner/withdrawal/history";
    public const string WITHDRAWAL_REQUEST_DETAIL = "shop-owner/withdrawal/{id:long}";
    public const string WITHDRAWAL_REQUEST_CANCEL = "shop-owner/withdrawal/cancel";
    public const string GET_WALLET_TRANSACTION = "shop-owner/walet-transaction";

    // Dashboard
    public const string ADMIN_ORDER_CHART = "admin/dashboard/order";
    public const string ADMIN_OVERVIEW_CHART = "admin/dashboard/overview";
    public const string ADMIN_REVENUE_CHART = "admin/dashboard/revenue-profit";

    // Commission Config
    public const string GET_COMMISSION_CONFIG = "admin/commission-config";
    public const string UPDATE_COMMISSION_CONFIG = "admin/commission-config/update";

    // Moderator
    public const string MANAGE_SHOP = "moderator/shop";
    public const string MANAGE_SHOP_DETAIL = "moderator/shop/{id:long}";
    public const string MANAGE_SHOP_UPDATE_STATUS = "moderator/shop/status";
    public const string MANAGE_SHOP_FOOD = "moderator/shop/{id:long}/food";

    public const string MANAGE_CUSTOMER = "moderator/customer";
    public const string MANAGE_CUSTOMER_DETAIL = "moderator/customer/{id:long}";
    public const string MANAGE_CUSTOMER_BAN_UNBAN = "moderator/customer/status";

    public const string MANAGE_WITHDRAWAL_REQUEST = "moderator/withdrawal-request";
    public const string MANAGE_WITHDRAWAL_REQUEST_DETAIL = "moderator/withdrawal-request/{id:long}";
    public const string MANAGE_WITHDRAWAL_UPDATE_STATUS = "moderator/withdrawal-request/status";

    public const string MANAGE_REPORT = "moderator/report";
    public const string MANAGE_REPORT_DETAIL = "moderator/report/{id:long}";
    public const string MANAGE_REPORT_UPDATE_STATUS = "moderator/report/status";

    public const string MODERATOR_ORDER = "moderator/order";
    public const string MODERATOR_ORDER_DETAIL = "moderator/order/{id:long}";

    public const string MODERATOR_DORMITORY = "moderator/dormitory";

    // Notification
    public const string NOTIFICATION_SHOP_STAFF = "shop-owner-staff/notification";
    public const string TOTAL_UNREAD_NOTIFICATION_SHOP_STAFF = "shop-owner-staff/notification/total-unread";
    public const string NOTIFICATION_UPDATE_SHOP_STAFF = "shop-owner-staff/notification";
    public const string NOTIFICATION_MARK_ALL_READ = "notification/mark-all-read";
    public const string CUSTOMER_NOTIFICATION = "customer/notification";
    public const string CUSTOMER_NOTIFICATION_TOTAL_UNREAD = "customer/notification/total-unread";

    // Chat
    public const string ORDER_INFOR_CHAT = "order/{id:long}/chat-info";
    public const string ORDER_LIST_INFOR_CHAT = "order/chat-info";

    // Admin
    public const string CREATE_MODERATOR_ACCOUNT = "admin/moderator/account";
    public const string UPDATE_MODERATOR_ACCOUNT = "admin/moderator/account/{id:long}";
    public const string GET_MODERATOR_ACCOUNT = "admin/moderator";
    public const string GET_MODERATOR_ACTIVITY_LOG = "admin/moderator/activity-log";
    public const string GET_MODERATOR_ACTIVITY_LOG_DETAIL = "admin/moderator/activity-log/{id:long}";
    public const string ADMIN_MANAGE_WITHDRAWAL_REQUEST_DETAIL = "admin/withdrawal-request/{id:long}";
    public const string GET_ORDER_DETAIL_FOR_ADMIN = "admin/order/{id:long}";
    public const string MANAGE_REPORT_DETAIL_ADMIN = "admin/report/{id:long}";
    public const string MANAGE_SHOP_DETAIL_ADMIN = "admin/shop/{id:long}";
    public const string MANAGE_CUSTOMER_DETAIL_ADMIN = "admin/customer/{id:long}";
}