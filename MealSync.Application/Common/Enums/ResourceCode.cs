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

    // Account
    [Description("Account-Avatar")]
    ACCOUNT_AVATAR,

    // Shop Owner
    [Description("Shop-Logo")]
    SHOP_LOGO,

    [Description("Shop-Banner")]
    SHOP_BANNER,

    // Notification
    [Description("Notification-Order-Reject")]
    NOTIFICATION_ORDER_REJECT,

    [Description("Notification-Order-Cancel")]
    NOTIFICATION_ORDER_CANCEL,
}