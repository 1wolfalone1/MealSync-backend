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
        var uniqOrder = new Dictionary<long, OrderDetailForShopResponse>();
        Func<OrderDetailForShopResponse, OrderDetailForShopResponse.CustomerInforInShoprderDetailForShop, OrderDetailForShopResponse.PromotionInShopOrderDetail, OrderDetailForShopResponse.ShopDeliveryStaffInShopOrderDetail,
            OrderDetailForShopResponse.FoodInShopOrderDetail, OrderDetailForShopResponse> map =
            (parent, child1, child2, child3, child4) =>
            {
                if (!uniqOrder.TryGetValue(parent.Id, out var order))
                {
                    parent.Customer = child1;
                    if (child2.Id != 0)
                    {
                        parent.Promotion = child2;
                    }

                    if (child3.DeliveryPackageId != 0 && (child3.Id != 0 || child3.IsShopOwnerShip))
                    {
                        parent.ShopDeliveryStaff = child3;
                    }

                    parent.OrderDetails.Add(child4);
                    uniqOrder.Add(parent.Id, parent);
                }
                else
                {
                    order.OrderDetails.Add(child4);
                    uniqOrder.Remove(order.Id);
                    uniqOrder.Add(order.Id, order);
                }

                return parent;
            };

        await _dapperService
            .SelectAsync<OrderDetailForShopResponse, OrderDetailForShopResponse.CustomerInforInShoprderDetailForShop, OrderDetailForShopResponse.PromotionInShopOrderDetail,
                OrderDetailForShopResponse.ShopDeliveryStaffInShopOrderDetail, OrderDetailForShopResponse.FoodInShopOrderDetail, OrderDetailForShopResponse>(
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
                DateFrom = request.DateFrom.HasValue ? request.DateFrom.Value.ToString("yyyy-MM-dd") : null,
                DateTo = request.DateTo.HasValue ? request.DateTo.Value.ToString("yyyy-MM-dd") : null,
            },
            "CustomerSection, PromotionSection, DeliveryPackageSection, OrderDetailSection").ConfigureAwait(false);

        var result = new PaginationResponse<OrderDetailForShopResponse>(
            uniqOrder.Values.ToList(), uniqOrder.Values.ToList().Count > 0 ? uniqOrder.Values.ToList().First().TotalPages : 0, request.PageIndex, request.PageSize
        );
        return Result.Success(result);
    }
}