using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Application.UseCases.Orders.Queries.ShopOrderByStatus;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Queries.ShopWebOrderByStatus;

public class GetShopWebOrderByStatusHandler : IQueryHandler<GetShopWebOrderByStatusQuery, Result>
{
    private readonly IDapperService _dapperService;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<GetShopOrderByStatusHandler> _logger;

    public GetShopWebOrderByStatusHandler(IDapperService dapperService, ICurrentPrincipalService currentPrincipalService, ILogger<GetShopOrderByStatusHandler> logger)
    {
        _dapperService = dapperService;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(GetShopWebOrderByStatusQuery request, CancellationToken cancellationToken)
    {
        var orderUniq = new Dictionary<long, OrderForShopWebByStatusResponse>();
        Func<OrderForShopWebByStatusResponse, OrderForShopWebByStatusResponse.CustomerInforInOrderForShop, OrderForShopWebByStatusResponse.ShopDeliveryStaffInOrderForShop, OrderForShopWebByStatusResponse.FoodInOrderForShop, OrderForShopWebByStatusResponse> map = (parent, child1, child2, child3) =>
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

        _logger.LogInformation($"Date filter request: {(request.IntendedReceiveDate != null ? request.IntendedReceiveDate.ToString() : string.Empty)}");
        await _dapperService.SelectAsync<OrderForShopWebByStatusResponse, OrderForShopWebByStatusResponse.CustomerInforInOrderForShop, OrderForShopWebByStatusResponse.ShopDeliveryStaffInOrderForShop,OrderForShopWebByStatusResponse.FoodInOrderForShop, OrderForShopWebByStatusResponse>(
            QueryName.GetListOrderForShopWebByStatus,
            map,
            new
            {
                ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                Status = request.Status,
                IntendedRecieveDate = request.IntendedReceiveDate != null ? request.IntendedReceiveDate.Value.ToString("yyyy-MM-dd") : null,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PhoneNumber = request.PhoneNumber,
                OrderId = request.Id != null ? request.Id.ToUpper() : null,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
            },
            "CustomerSection, ShopDeliverySection, FoodSection").ConfigureAwait(false);

        var result = new PaginationResponse<OrderForShopWebByStatusResponse>(
            orderUniq.Values.ToList(), orderUniq.Values.ToList().Count > 0 ? orderUniq.Values.ToList().First().TotalPages : 0, request.PageIndex, request.PageSize
        );
        return Result.Success(result);
    }
}