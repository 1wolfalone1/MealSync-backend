using System.ComponentModel;

namespace MealSync.Application.Common.Enums;

public enum ResourceCode
{
    // Email
    [Description("Email-SubjectRegisterVerification")]
    EMAIL_SUBJECT_REGISTER_VERIFICATION,

    [Description("Email-RegisterVerification")]
    EMAIL_REGISTER_VERIFICATION,

    [Description("Email-SubjectForgotPasswordVerification")]
    EMAIL_SUBJECT_FORGOT_PASSWORD_VERIFICATION,

    [Description("Email-ForgotPasswordVerification")]
    EMAIL_FORGOT_PASSWORD_VERIFICATION,

    [Description("Email-SubjectOldEmailVerification")]
    EMAIL_SUBJECT_OLD_EMAIL_VERIFICATION,

    [Description("Email-OldEmailVerification")]
    EMAIL_OLD_EMAIL_VERIFICATION,

    [Description("Email-SubjectUpdateEmailVerification")]
    EMAIL_SUBJECT_UPDATE_EMAIL_VERIFICATION,

    [Description("Email-UpdateEmailVerification")]
    EMAIL_UPDATE_EMAIL_VERIFICATION,

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

    [Description("Email-SubjectLimitAvailableAmount")]
    EMAIL_SUBJECT_LIMIT_AVAILABLE_AMOUNT,

    [Description("Email-Body-LimitAvailableAmount")]
    EMAIL_BODY_LIMIT_AVAILABLE_AMOUNT,

    [Description("Email-SubjectBanCustomerAccount")]
    EMAIL_SUBJECT_BAN_CUSTOMER_ACCOUNT,

    [Description("Email-Body-BanCustomerAccount")]
    EMAIL_BODY_BAN_CUSTOMER_ACCOUNT,

    [Description("Email-SubjectApproveShop")]
    EMAIL_SUBJECT_APPROVE_SHOP,

    [Description("Email-Body-ApproveShop")]
    EMAIL_BODY_APPROVE_SHOP,

    [Description("Email-SubjectBannedShopWithReason")]
    EMAIL_SUBJECT_BANNED_SHOP_WITH_REASON,

    [Description("Email-SubjectBanningShopWithReason")]
    EMAIL_SUBJECT_BANNING_SHOP_WITH_REASON,

    [Description("Email-Body-BanShopWithReason")]
    EMAIL_BODY_BAN_SHOP_WITH_REASON,

    [Description("Email-SubjectUnShopWithReason")]
    EMAIL_SUBJECT_UN_BAN_SHOP_WITH_REASON,

    [Description("Email-Body-UnBanShopWithReason")]
    EMAIL_BODY_UN_BAN_SHOP_WITH_REASON,

    [Description("Email-SubjectBannedCustomerWithReason")]
    EMAIL_SUBJECT_BANNED_CUSTOMER_WITH_REASON,

    [Description("Email-SubjectBanningCustomerWithReason")]
    EMAIL_SUBJECT_BANNING_CUSTOMER_WITH_REASON,

    [Description("Email-Body-BanCustomerWithReason")]
    EMAIL_BODY_BAN_CUSTOMER_WITH_REASON,

    [Description("Email-SubjectUnBanCustomerWithReason")]
    EMAIL_SUBJECT_UN_BAN_CUSTOMER_WITH_REASON,

    [Description("Email-Body-UnBanCustomerWithReason")]
    EMAIL_BODY_UN_BAN_CUSTOMER_WITH_REASON,

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

    [Description("Notification-Order-Customer-Cancel")]
    NOTIFICATION_ORDER_CUSTOMER_CANCEL,

    [Description("Notification-Order-Customer-Confirm-Completed")]
    NOTIFICATION_ORDER_CUSTOMER_CONFIRM_COMPLETED,

    [Description("Notification-DeliveryFail-AutoByBatch")]
    NOTIFICATION_DELIVERY_FAIL_AUTO_BY_BATCH,

    [Description("Notification-AvailableAmountLessThanLimit")]
    NOTIFICATION_AVAILABLE_AMOUNT_LESS_THAN_LIMIT,

    [Description("Notification-WarningFlagCustomer")]
    NOTIFICATION_WARNING_FLAG_CUSTOMER,

    [Description("Notification-Wallet-ShopReceiveIncomingAmount")]
    NOTIFICATION_WALLET_SHOP_RECEIVE_INCOMMING_AMOUNT,

    [Description("Notification-Wallet-TakeCommissionFeeFromShopWallet")]
    NOTIFICATION_WALLET_TAKE_COMMISISSION_FEE_FROM_SHOP_WALLET,

    [Description("Notification-Order-Of-Customer-System-Cancel")]
    NOTIFICATION_ORDER_OF_CUSTOMER_SYSTEM_CANCEL,

    [Description("Notification-Order-Of-Shop-System-Cancel")]
    NOTIFICATION_ORDER_OF_SHOP_SYSTEM_CANCEL,

    [Description("Notification-RefundOrderForCustomer")]
    NOTIFICATION_REFUND_ORDER_FOR_CUSTOMER,

    [Description("Notification-UnderReviewReport")]
    NOTIFICATION_UNDER_REVIEW_REPORT,

    [Description("Notification-ApproveReport")]
    NOTIFICATION_APPROVE_REPORT,

    [Description("Notification-RejectReport")]
    NOTIFICATION_REJECT_REPORT,
}