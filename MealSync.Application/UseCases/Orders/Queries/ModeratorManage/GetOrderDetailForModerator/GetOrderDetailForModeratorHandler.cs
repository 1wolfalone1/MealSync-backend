using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Orders.Queries.ModeratorManage.GetOrderDetailForModerator;

public class GetOrderDetailForModeratorHandler : IQueryHandler<GetOrderDetailForModeratorQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IDapperService _dapperService;
    private readonly IOrderRepository _orderRepository;

    public GetOrderDetailForModeratorHandler(ICurrentPrincipalService currentPrincipalService, IModeratorDormitoryRepository moderatorDormitoryRepository, IDapperService dapperService, IOrderRepository orderRepository)
    {
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _dapperService = dapperService;
        _orderRepository = orderRepository;
    }

    public async Task<Result<Result>> Handle(GetOrderDetailForModeratorQuery request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var uniqOrder = new Dictionary<long, OrderDetailForModeratorResponse>();
        Func<OrderDetailForModeratorResponse, OrderDetailForModeratorResponse.CustomerInforInShopOrderDetailForModerator, OrderDetailForModeratorResponse.ShopInforForOrderDetailMod, OrderDetailForModeratorResponse.PromotionInModeratorOrderDetail, OrderDetailForModeratorResponse.ShopDeliveryStaffInModeratorOrderDetail,
            OrderDetailForModeratorResponse.FoodInModeratorOrderDetail, OrderDetailForModeratorResponse> map =
            (parent, child1, child2, child3, child4, child5) =>
            {
                if (!uniqOrder.TryGetValue(parent.Id, out var order))
                {
                    parent.Customer = child1;
                    parent.Shop = child2;
                    if (child3.Id != 0)
                    {
                        parent.Promotion = child3;
                    }

                    if (child4.DeliveryPackageId != 0 && (child4.Id != 0 || child4.IsShopOwnerShip))
                    {
                        parent.ShopDeliveryStaff = child4;
                    }

                    parent.OrderDetails.Add(child5);
                    uniqOrder.Add(parent.Id, parent);
                }
                else
                {
                    order.OrderDetails.Add(child5);
                    uniqOrder.Remove(order.Id);
                    uniqOrder.Add(order.Id, order);
                }

                return parent;
            };

        await _dapperService
            .SelectAsync<OrderDetailForModeratorResponse, OrderDetailForModeratorResponse.CustomerInforInShopOrderDetailForModerator, OrderDetailForModeratorResponse.ShopInforForOrderDetailMod, OrderDetailForModeratorResponse.PromotionInModeratorOrderDetail, OrderDetailForModeratorResponse.ShopDeliveryStaffInModeratorOrderDetail,
                OrderDetailForModeratorResponse.FoodInModeratorOrderDetail, OrderDetailForModeratorResponse>(
                QueryName.GetOrderDetailForModeratorById,
                map,
                new
                {
                    OrderId = request.OrderId,
                },
                "CustomerSection, ShopSection, PromotionSection, DeliveryPackageSection, OrderDetailSection").ConfigureAwait(false);

        return Result.Success(uniqOrder.Values.FirstOrDefault());
    }

    private void Validate(GetOrderDetailForModeratorQuery request)
    {
        var order = _orderRepository.GetOrderWithDormitoryById(request.OrderId);
        if (order == null)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId }, HttpStatusCode.NotFound);

        var modDormitory = _moderatorDormitoryRepository.Get(mod => mod.ModeratorId == _currentPrincipalService.CurrentPrincipalId).ToList();
        if (order.Building != null && modDormitory != null && !modDormitory.Any(mod => mod.DormitoryId == order.Building.DormitoryId))
            throw new InvalidBusinessException(MessageCode.E_ORDER_MODERATOR_NOT_HAVE_AUTHOR_TO_ACCESS.GetDescription(), new object[] { request.OrderId });
    }
}