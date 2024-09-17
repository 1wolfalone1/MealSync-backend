using System.ComponentModel;

namespace MealSync.Application.Common.Enums;

public enum MessageCode
{
    [Description("Email bắt buộc nhập")] VL00001,

    [Description("Password bắt buộc nhập")] VL00002,

    [Description("Email hoặc mật khẩu không chính xác")] BU00001,

    [Description("Tài khoản của bạn chưa được xác thực")] BU00002,

    [Description("Tài khoản bạn đã bị khóa")] BU00003,

}
