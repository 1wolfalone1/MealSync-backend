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

    // Account
    [Description("Account-Avatar")]
    ACCOUNT_AVATAR,

    // Shop Owner
    [Description("Shop-Logo")]
    SHOP_LOGO,

    [Description("Shop-Banner")]
    SHOP_BANNER,
}