using System.ComponentModel;

namespace MealSync.Application.Common.Enums;

public enum ResourceCode
{
    // Email
    [Description("Email-SubjectRegisterVerification")]
    EMAIL_SUBJECT_REGISTER_VERIFICATION,

    [Description("Email-RegisterVerification")]
    EMAIL_REGISTER_VERIFICATION,

    [Description("Email-ForgotPasswordVerification")]
    EMAIL_FORGOT_PASSWORD_VERIFICATION,

    [Description("Email-SubjectForgotPasswordVerification")]
    EMAIL_SUBJECT_FORGOT_PASSWORD_VERIFICATION,

    [Description("Email-Subject-AnnounceWarningForShop")]
    EMAIL_SUBJECT_ANNOUCE_WARNING_FOR_SHOP,

    [Description("Email-Body-AnnounceWarningForShop")]
    EMAIL_BODY_ANNOUCE_WARNING_FOR_SHOP,

    [Description("Email-Subject-AnnounceApplyFlagForShop")]
    EMAIL_SUBJECT_ANNOUCE_APPLY_FLAG_FOR_SHOP,

    [Description("Email-Body-AnnouceApplyFlagForShop")]
    EMAIL_BODY_ANNOUCE_APPLY_FLAG_FOR_SHOP,

    [Description("Email-Subject-AnnouceRefundOrderFail")]
    EMAIL_SUBJECT_ANNOUCE_REFUND_ORDER_FAIL,

    [Description("Email-Body-AnnouceRefundOrderFail")]
    EMAIL_BODY_ANNOUCE_REFUND_ORDER_FAIL,

    [Description("Email-Subject-AccountEnoughFlagForBan")]
    EMAIL_SUBJECT_ACCOUNT_ENOUGH_FLAG_FOR_BAN,

    [Description("Email-Body-Account-EnoughFlagForBan")]
    EMAIL_BODY_ACCOUNT_ENOUGH_FLAG_FOR_BAN,

    [Description("Email-Subject-WithdrawalRequest")]
    EMAIL_SUBJECT_WITHDRAWAL_REQUEST,

    [Description("Email-Body-WithdrawalRequest")]
    EMAIL_BODY_WITHDRAWAL_REQUEST,

    // Account
    [Description("Account-Avatar")]
    ACCOUNT_AVATAR,

    // Shop Owner
    [Description("Shop-Logo")]
    SHOP_LOGO,

    [Description("Shop-Banner")]
    SHOP_BANNER,

    // Notification
    [Description("Notification-Order-Pending")]
    NOTIFICATION_ORDER_PENDING,

    [Description("Notification-Order-Confirmed")]
    NOTIFICATION_ORDER_CONFIRMED,

    [Description("Notification-Order-Reject")]
    NOTIFICATION_ORDER_REJECT,

    [Description("Notification-Order-Cancel")]
    NOTIFICATION_ORDER_CANCEL,

    [Description("Notification-Order-Confirm")]
    NOTIFICATION_ORDER_CONFIRM,

    [Description("Notification-Order-RefundFail")]
    NOTIFICATION_ORDER_REFUND_FAIL,

    [Description("Notification-Order-Preparing")]
    NOTIFICATION_ORDER_PREPARING,

    [Description("Notification-Order-CustomerDelivered")]
    NOTIFICATION_ORDER_CUSTOMER_DELIVERED,

    [Description("Notification-Order-ShopDelivered")]
    NOTIFICATION_ORDER_SHOP_DELIVERED,

    [Description("Notification-Order-CustomerDelivering")]
    NOTIFICATION_ORDER_CUSTOMER_DELIVERING,

    [Description("Notification-Order-AssignedToShopStaff")]
    NOTIFICATION_ORDER_ASSIGNED_TO_SHOP_STAFF,

    [Description("Notification-Order-DeliveryFailToCustomer")]
    NOTIFICATION_ORDER_DELIVERY_FAIL_TO_CUSTOMER,

    [Description("Notification-Order-DeliveryFailToShop")]
    NOTIFICATION_ORDER_DELIVERY_FAIL_TO_SHOP,

    [Description("Notification-Order-DeliveryFailToModerator")]
    NOTIFICATION_ORDER_DELIVERY_FAIL_TO_MODERATOR,
}