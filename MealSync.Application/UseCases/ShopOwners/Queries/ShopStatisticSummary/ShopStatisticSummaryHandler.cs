using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopOwners.Queries.ShopStatisticSummary;

public class ShopStatisticSummaryHandler : IQueryHandler<ShopStatisticSummaryQuery, Result>
{
    private readonly IDapperService _dapperService;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public ShopStatisticSummaryHandler(IDapperService dapperService, ICurrentPrincipalService currentPrincipalService)
    {
        _dapperService = dapperService;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(ShopStatisticSummaryQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        var startDate = new DateTime(now.Year, now.Month, 1).ToString("yyyy-MM-dd");
        var endDate = now.ToString("yyyy-MM-dd");

        var orderStatisticInMonth = await _dapperService.SingleOrDefaultAsync<OrderStatisticInMonthSummaryDto>(QueryName.GetOrderStatisticsInMonthSummary, new
        {
            ShopId = shopId,
            CustomerCancel = OrderIdentityCode.ORDER_IDENTITY_CUSTOMER_CANCEL.GetDescription(),
            ShopCancel = OrderIdentityCode.ORDER_IDENTITY_SHOP_CANCEL.GetDescription(),
            DeliveryFailByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription(),
            DeliveryFailByShop = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription(),
            DeliveredReportedByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription(),
            DeliveryFailReportedByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_REPORTED_BY_CUSTOMER.GetDescription(),
            PaymentOnlineList = new PaymentMethods[] { PaymentMethods.VnPay },
            StartDate = startDate,
            EndDate = endDate,
        }).ConfigureAwait(false);

        var orderStatisticInToday = await _dapperService.SingleOrDefaultAsync<OrderStatisticInTodaySummaryDto>(QueryName.GetOrderStatisticsInTodaySummary, new
        {
            ShopId = shopId,
            Today = now.ToString("yyyy-MM-dd"),
        }).ConfigureAwait(false);
        orderStatisticInToday.Date = now.ToString("yyyy-MM-dd");

        return Result.Success(new ShopStatisticSummaryResponse
        {
            OrderStatisticInToday = orderStatisticInToday,
            OrderStatisticInMonth = new ShopStatisticSummaryResponse.OrderStatisticInMonthResponse
            {
                Month = now.Month,
                StartDate = startDate,
                EndDate = endDate,
                Revenue = orderStatisticInMonth.Revenue,
                TotalSuccess = orderStatisticInMonth.TotalDeliveredCompleted,
                TotalFailOrRefund = orderStatisticInMonth.TotalFailDeliveredByCustomerCompleted + orderStatisticInMonth.TotalFailDeliveredByShopCompleted + orderStatisticInMonth.TotalReportResolvedHaveRefund,
                TotalCancelOrReject = orderStatisticInMonth.TotalCancelByCustomer + orderStatisticInMonth.TotalCancelByShop + orderStatisticInMonth.TotalReject,
            },
        });
    }
}