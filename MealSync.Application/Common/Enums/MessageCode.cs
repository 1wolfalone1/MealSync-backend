using System.ComponentModel;

namespace MealSync.Application.Common.Enums;

public enum MessageCode
{
    [Description("E-Account-InvalidUserNamePassword")]
    E_ACCOUNT_INVALID_USERNAME_PASSWORD,

    [Description("E-Account-Unverified")]
    E_ACCOUNT_UNVERIFIED,

    [Description("E-Account-Banned")]
    E_ACCOUNT_BANNED,

    [Description("E-Account-InvalidRole")]
    E_ACCOUNT_INVALID_ROLE,

    [Description("E-Dormitory-NotFound")]
    E_DORMITORY_NOT_FOUND,

    [Description("E-Account-PhoneNumberExist")]
    E_ACCOUNT_PHONE_NUMBER_EXIST,

    [Description("E-Account-EmailExist")]
    E_ACCOUNT_EMAIL_EXIST,

    [Description("I-Account-RegisterSuccessfully")]
    I_ACCOUNT_REGISTER_SUCCESSFULLY,

    [Description("I-Email-SubjectRegisterVerification")]
    I_EMAIL_SUBJECT_REGISTER_VERIFICATION,

    [Description("I-Email-RegisterVerification")]
    I_EMAIL_REGISTER_VERIFICATION,

    [Description("I-Email-ForgotPasswordVerification")]
    I_EMAIL_FORGOT_PASSWORD_VERIFICATION,

    [Description("I-Email-SubjectForgotPasswordVerification")]
    I_EMAIL_SUBJECT_FORGOT_PASSWORD_VERIFICATION,

    [Description("I-Account-Avatar")]
    I_ACCOUNT_AVATAR,
}