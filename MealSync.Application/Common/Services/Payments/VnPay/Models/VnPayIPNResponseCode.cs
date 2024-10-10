using System.ComponentModel;

namespace MealSync.Application.Common.Services.Payments.VnPay.Models;

public enum VnPayIPNResponseCode
{
    [Description("Confirm Success")]
    CODE_00 = 00,

    [Description("Order not found")]
    CODE_01 = 01,

    [Description("Order already confirmed")]
    CODE_02 = 02,

    [Description("invalid amount")]
    CODE_04 = 04,

    [Description("Invalid signature")]
    CODE_97 = 97,

    [Description("Input data required")]
    CODE_99 = 99,
}