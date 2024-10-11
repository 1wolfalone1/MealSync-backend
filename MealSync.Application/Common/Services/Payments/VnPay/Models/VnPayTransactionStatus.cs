using System.ComponentModel;

namespace MealSync.Application.Common.Services.Payments.VnPay.Models;

public enum VnPayTransactionStatus
{
    [Description("Giao dịch thanh toán thành công")]
    CODE_00 = 00,

    [Description("Giao dịch chưa hoàn tất")]
    CODE_01 = 01,

    [Description("Giao dịch bị lỗi")]
    CODE_02 = 02,

    [Description("Giao dịch đảo (Khách hàng đã bị trừ tiền tại Ngân hàng nhưng GD chưa thành công ở VNPAY)")]
    CODE_04 = 04,

    [Description("VNPAY đang xử lý giao dịch này (GD hoàn tiền)")]
    CODE_05 = 05,

    [Description("VNPAY đã gửi yêu cầu hoàn tiền sang Ngân hàng (GD hoàn tiền)")]
    CODE_06 = 06,

    [Description("Giao dịch bị nghi ngờ gian lận")]
    CODE_07 = 07,

    [Description("GD Hoàn trả bị từ chối")]
    CODE_09 = 09,
}