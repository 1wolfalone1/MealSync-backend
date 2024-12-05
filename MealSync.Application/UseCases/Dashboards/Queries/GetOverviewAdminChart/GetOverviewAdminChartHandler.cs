using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Dashboards.Models;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Dashboards.Queries.GetOverviewAdminChart;

public class GetOverviewAdminChartHandler : IQueryHandler<GetOverviewAdminChartQuery, Result>
{
    private readonly IDapperService _dapperService;

    public GetOverviewAdminChartHandler(IDapperService dapperService)
    {
        _dapperService = dapperService;
    }

    public async Task<Result<Result>> Handle(GetOverviewAdminChartQuery request, CancellationToken cancellationToken)
    {
        var numberDiffDay = (request.DateTo - request.DateFrom).Days + 1;

        var currentOverview = _dapperService.SingleOrDefault<OverviewChartResponse>(QueryName.GetOverviewAdminChart,
            new
            {
                DateFrom = request.DateFrom.ToString("yyyy-MM-dd"),
                DateTo = request.DateTo.ToString("yyyy-MM-dd"),
                PaymentOnlineList = new PaymentMethods[] { PaymentMethods.VnPay },
                DeliveryFailReportedByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER,
                DeliveredReportedByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER,
                DeliveryFailByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER,
                CustomerCancel = OrderIdentityCode.ORDER_IDENTITY_CUSTOMER_CANCEL,
                ShopCancel = OrderIdentityCode.ORDER_IDENTITY_SHOP_CANCEL,
                DeliveryFailByShop = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP,
            });

        var previousOverview = _dapperService.SingleOrDefault<OverviewChartResponse>(QueryName.GetOverviewAdminChart,
            new
            {
                DateFrom = request.DateFrom.AddDays(-numberDiffDay).ToString("yyyy-MM-dd"),
                DateTo = request.DateFrom.ToString("yyyy-MM-dd"),
                PaymentOnlineList = new PaymentMethods[] { PaymentMethods.VnPay },
                DeliveryFailReportedByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER,
                DeliveredReportedByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER,
                DeliveryFailByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER,
                CustomerCancel = OrderIdentityCode.ORDER_IDENTITY_CUSTOMER_CANCEL,
                ShopCancel = OrderIdentityCode.ORDER_IDENTITY_SHOP_CANCEL,
                DeliveryFailByShop = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP,
            });

        currentOverview.CalTotalOrderRate(previousOverview.TotalOrder);
        currentOverview.CalTotalChargeFeeRate(previousOverview.TotalChargeFee);
        currentOverview.CalTotalUserRate(previousOverview.TotalUser);
        currentOverview.CalTotalTradingRate(previousOverview.TotalTradingAmount);
        currentOverview.NumDayCompare = numberDiffDay;

        return Result.Success(currentOverview);
    }
}