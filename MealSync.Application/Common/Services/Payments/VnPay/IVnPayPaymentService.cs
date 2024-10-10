using MealSync.Application.Common.Services.Payments.VnPay.Models;
using MealSync.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace MealSync.Application.Common.Services.Payments.VnPay;

public interface IVnPayPaymentService
{
    Task<string> CreatePaymentUrl(Payment payment);

    Task<VnPayRefundResponse> CreateRefund(Payment payment);

    Task<VnPayIPNResponse> GetIPN(IQueryCollection queryParams, Payment payment);
}