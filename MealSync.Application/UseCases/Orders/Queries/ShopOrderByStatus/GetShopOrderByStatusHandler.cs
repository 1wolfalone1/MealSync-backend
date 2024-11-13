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
            QueryName.GetListOrderForShopByStatus,
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
            "CustomerSection, PromotionSection, DeliveryPackageSection, OrderDetailSection").ConfigureAwait(false);

        var result = new PaginationResponse<OrderDetailForShopResponse>(
            uniqOrder.Values.ToList(), uniqOrder.Values.ToList().Count > 0 ? uniqOrder.Values.ToList().First().TotalPages : 0, request.PageIndex, request.PageSize
        );
        return Result.Success(result);
    }
}