using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Queries.ShopOrderByStatus;

public class GetShopOrderByStatusHandler : IQueryHandler<GetShopOrderByStatusQuery, Result>
{
    private readonly IDapperService _dapperService;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<GetShopOrderByStatusHandler> _logger;

    public GetShopOrderByStatusHandler(IDapperService dapperService, ICurrentPrincipalService currentPrincipalService, ILogger<GetShopOrderByStatusHandler> logger)
    {
        _dapperService = dapperService;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(GetShopOrderByStatusQuery request, CancellationToken cancellationToken)
    {
        var orderUniq = new Dictionary<long, OrderForShopByStatusResponse>();
        Func<OrderForShopByStatusResponse, OrderForShopByStatusResponse.CustomerInforInOrderForShop, OrderForShopByStatusResponse.ShopDeliveryStaffInOrderForShop, OrderForShopByStatusResponse.FoodInOrderForShop, OrderForShopByStatusResponse> map = (parent, child1, child2, child3) =>
        {
            if (!orderUniq.TryGetValue(parent.Id, out var order))
            {
                parent.Customer = child1;
                if (child2.DeliveryPackageId != 0 && (child2.Id != 0 || child2.IsShopOwnerShip))
                {
                    parent.ShopDeliveryStaff = child2;
                }

                parent.Foods.Add(child3);
                orderUniq.Add(parent.Id, parent);
            }
            else
            {
                order.Foods.Add(child3);
                orderUniq.Remove(order.Id);
                orderUniq.Add(order.Id, order);
            }

            return parent;
        };

        _logger.LogInformation($"Date filter request: {(request.IntendedRecieveDate != null ? request.IntendedRecieveDate.ToString() : string.Empty)}");
        await _dapperService.SelectAsync<OrderForShopByStatusResponse, OrderForShopByStatusResponse.CustomerInforInOrderForShop, OrderForShopByStatusResponse.ShopDeliveryStaffInOrderForShop,OrderForShopByStatusResponse.FoodInOrderForShop, OrderForShopByStatusResponse>(
            QueryName.GetListOrderForShopByStatus,
            map,
            new
            {
                ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                Status = request.Status,
                IntendedRecieveDate = request.IntendedRecieveDate != null ? request.IntendedRecieveDate.Value.ToString("yyyy-M-d") : null,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PhoneNumber = request.PhoneNumber,
                OrderId = request.Id != null ? request.Id.ToUpper() : null,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
            },
            "CustomerSection, ShopDeliverySection, FoodSection").ConfigureAwait(false);

        var result = new PaginationResponse<OrderForShopByStatusResponse>(
            orderUniq.Values.ToList(), orderUniq.Values.ToList().Count > 0 ? orderUniq.Values.ToList().First().TotalPages : 0, request.PageIndex, request.PageSize
        );
        return Result.Success(result);
    }
}