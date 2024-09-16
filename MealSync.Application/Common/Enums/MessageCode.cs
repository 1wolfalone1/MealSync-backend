using System.ComponentModel;

namespace MealSync.Application.Common.Enums;

public enum MessageCode
{
    [Description("Email hoặc mật khẩu không chính xác")] BU00001,

    [Description("Tài khoản của bạn chưa được xác thực")] BU00002,

    [Description("Tài khoản bạn đã bị khóa")] BU00003,

}
