using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopOwners.Queries.ShopStatistics;

public class ShopStatisticHandler : IQueryHandler<ShopStatisticQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IDapperService _dapperService;

    public ShopStatisticHandler(ICurrentPrincipalService currentPrincipalService, IDapperService dapperService)
    {
        _currentPrincipalService = currentPrincipalService;
        _dapperService = dapperService;
    }

    public async Task<Result<Result>> Handle(ShopStatisticQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        // return Result.Success(_orderRepository.GetShopOrderStatistic(shopId, request.StartDate, request.EndDate));

        var foods = await _dapperService.SelectAsync<TopFoodOrderDto>(QueryName.GetTopFoodOrderOfShop, new
        {
            ShopId = shopId,
            NumberTopProduct = request.NumberTopProduct,
            StartDate = request.StartDate.ToString("yyyy-MM-dd"),
            EndDate = request.EndDate.ToString("yyyy-MM-dd"),
        }).ConfigureAwait(false);

        var orderStatistic = await _dapperService.SingleOrDefaultAsync<OrderStatisticDto>(QueryName.GetOrderStatistics, new
        {
            ShopId = shopId,
            CustomerCancel = OrderIdentityCode.ORDER_IDENTITY_CUSTOMER_CANCEL.GetDescription(),
            ShopCancel = OrderIdentityCode.ORDER_IDENTITY_SHOP_CANCEL.GetDescription(),
            DeliveryFailByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription(),
            DeliveryFailByShop = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription(),
            DeliveredReportedByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription(),
            DeliveryFailByCustomerReportedByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription(),
            DeliveryFailByShopReportedByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP_REPORTED_BY_CUSTOMER.GetDescription(),
            PaymentOnlineList = new PaymentMethods[] { PaymentMethods.VnPay },
            StartDate = request.StartDate.ToString("yyyy-MM-dd"),
            EndDate = request.EndDate.ToString("yyyy-MM-dd"),
        }).ConfigureAwait(false);

        return Result.Success(new ShopStatisticsResponse
        {
            StartDate = request.StartDate.ToString("yyyy-MM-dd"),
            EndDate = request.EndDate.ToString("yyyy-MM-dd"),
            TotalOrderDone = orderStatistic.TotalOrderDone,
            TotalOrderInProcess = orderStatistic.TotalOrderInProcess,
            SuccessfulOrderPercentage = orderStatistic.SuccessfulOrderPercentage,
            Revenue = orderStatistic.Revenue,
            TotalSuccess = orderStatistic.TotalDeliveredCompleted,
            TotalFailOrRefund = orderStatistic.TotalFailDeliveredByCustomerCompleted + orderStatistic.TotalFailDeliveredByShopCompleted + orderStatistic.TotalReportResolvedHaveRefund,
            TotalCancelOrReject = orderStatistic.TotalCancelByCustomer + orderStatistic.TotalCancelByShop + orderStatistic.TotalReject,
            Foods = foods.ToList(),
        });
    }
}