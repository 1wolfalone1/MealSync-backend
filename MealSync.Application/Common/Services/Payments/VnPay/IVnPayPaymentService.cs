using MealSync.Application.Common.Services.Payments.VnPay.Models;
using MealSync.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace MealSync.Application.Common.Services.Payments.VnPay;

public interface IVnPayPaymentService
{
    Task<string> CreatePaymentOrderUrl(Payment payment);

    Task<string> CreatePaymentDepositUrl(Deposit deposit);

    Task<VnPayRefundResponse> CreateRefund(Payment payment);

    Task<VnPayIPNResponse> GetIPNPaymentOrder(IQueryCollection queryParams, Payment payment);

    Task<VnPayIPNResponse> GetIPNDeposit(IQueryCollection queryParams, Deposit deposit);

    Task<string> GetPaymentType(IQueryCollection queryParams);
}