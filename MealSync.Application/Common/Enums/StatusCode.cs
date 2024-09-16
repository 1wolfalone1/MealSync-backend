using System.ComponentModel;

namespace MealSync.Application.Common.Enums;

public enum StatusCode
{
    [Description("400")] BAD_REQUEST = 400,

    [Description("401")] UNAUTHORIZED = 401,

    [Description("500")] INTERNAL_SERVER_ERROR = 500,
}