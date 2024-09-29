using System.ComponentModel;

namespace MealSync.Application.Common.Enums;

public enum MessageCode
{
    // Account
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

    // Category
    [Description("E-Category-NotFound")]
    E_CATEGORY_NOT_FOUND,

    // Operating Day
    [Description("E-OperatingDay-NotFound")]
    E_OPERATING_DAY_NOT_FOUND,

    // Operating Frame
    [Description("E-OperatingFrame-HasOverlapping")]
    E_OPERATING_FRAME_HAS_OVERLAPPING,

    [Description("E-OperatingFrame-HasNotActiveTime")]
    E_OPERATING_FRAME_HAS_NOT_ACTIVE_TIME,
}