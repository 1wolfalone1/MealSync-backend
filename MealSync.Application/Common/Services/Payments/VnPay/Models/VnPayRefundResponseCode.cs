using System.ComponentModel;

namespace MealSync.Application.Common.Services.Payments.VnPay.Models;

public enum VnPayRefundResponseCode
{
    [Description("Yêu cầu thành công")]
    CODE_00 = 00,

    [Description("Không tìm thấy giao dịch yêu cầu hoàn trả")]
    CODE_91 = 91,

    [Description("Giao dịch đã được gửi yêu cầu hoàn tiền trước đó. Yêu cầu này VNPAY đang xử lý")]
    CODE_94 = 94,

    [Description("Giao dịch này không thành công bên VNPAY. VNPAY từ chối xử lý yêu cầu")]
    CODE_95 = 95,
}