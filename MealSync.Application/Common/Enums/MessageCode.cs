using System.ComponentModel;

namespace MealSync.Application.Common.Enums;

public enum MessageCode
{
    [Description("E-Account-InvalidUserNamePassword")]
    E_ACCOUNT_INVALID_USERNAME_PASSWORD,

    [Description("E-Account-Unverified")] E_ACCOUNT_UNVERIFIED,

    [Description("E-Account-Banned")] E_ACCOUNT_BANNED,

    [Description("E-Account-InvalidRole")] E_ACCOUNT_INVALID_ROLE,
}