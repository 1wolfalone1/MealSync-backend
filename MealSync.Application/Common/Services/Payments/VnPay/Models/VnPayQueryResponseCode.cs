using System.ComponentModel;

namespace MealSync.Application.Common.Services.Payments.VnPay.Models;

public enum VnPayQueryResponseCode
{
    [Description("Yêu cầu thành công")]
    CODE_00 = 00,

    [Description("Không tìm thấy giao dịch yêu cầu")]
    CODE_91 = 91,

    [Description("Yêu cầu trùng lặp, duplicate request trong thời gian giới hạn của API")]
    CODE_94 = 94,
}