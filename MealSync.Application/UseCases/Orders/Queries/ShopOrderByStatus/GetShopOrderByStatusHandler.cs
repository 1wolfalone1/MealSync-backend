using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;

namespace MealSync.Application.UseCases.Orders.Queries.ShopOrderByStatus;

public class GetShopOrderByStatusHandler : IQueryHandler<GetShopOrderByStatusQuery, Result>
{
    private readonly IDapperService _dapperService;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetShopOrderByStatusHandler(IDapperService dapperService, ICurrentPrincipalService currentPrincipalService)
    {
        _dapperService = dapperService;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetShopOrderByStatusQuery request, CancellationToken cancellationToken)
    {
        var orderUniq = new Dictionary<long, OrderForShopByStatusResponse>();
        Func<OrderForShopByStatusResponse, OrderForShopByStatusResponse.CustomerInforInOrderForShop, OrderForShopByStatusResponse.FoodInOrderForShop, OrderForShopByStatusResponse> map = (parent, child1, child2) =>
        {
            if (!orderUniq.TryGetValue(parent.Id, out var order))
            {
                parent.CustomerInfor = child1;
                parent.Foods.Add(child2);
                orderUniq.Add(parent.Id, parent);
            }
            else
            {
                order.Foods.Add(child2);
                orderUniq.Remove(order.Id);
                orderUniq.Add(order.Id, order);
            }

            return parent;
        };

        await _dapperService.SelectAsync<OrderForShopByStatusResponse, OrderForShopByStatusResponse.CustomerInforInOrderForShop, OrderForShopByStatusResponse.FoodInOrderForShop, OrderForShopByStatusResponse>(
            QueryName.GetListOrderForShopByStatus,
            map,
            new
            {
                ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                Status = request.Status,
                IntendedRecieveDate = request.IntendedRecieveDate.ToString("yyyy-M-d"),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PhoneNumber = request.PhoneNumber,
                OrderId = request.Id,
                Offset = (request.PageIndex - 1) * request.PageSize,
                PageSize = request.PageSize,
            },
            "CustomerSection, FoodSection").ConfigureAwait(false);

        return Result.Success(orderUniq.Values.ToList());
    }
}