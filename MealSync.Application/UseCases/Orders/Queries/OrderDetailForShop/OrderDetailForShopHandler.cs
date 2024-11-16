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

namespace MealSync.Application.UseCases.Orders.Queries.OrderDetailForShop;

public class OrderDetailForShopHandler : IQueryHandler<OrderDetailForShopQuery, Result>
{
    private readonly IDapperService _dapperService;
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;

    public OrderDetailForShopHandler(IDapperService dapperService, IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService, ICurrentAccountService currentAccountService, IShopDeliveryStaffRepository shopDeliveryStaffRepository)
    {
        _dapperService = dapperService;
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _currentAccountService = currentAccountService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
    }

    public async Task<Result<Result>> Handle(OrderDetailForShopQuery request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

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
                QueryName.GetShopOrderDetail,
                map,
                new
                {
                    OrderId = request.Id,
                },
                "CustomerSection, PromotionSection, DeliveryPackageSection, OrderDetailSection").ConfigureAwait(false);

        return Result.Success(uniqOrder.Values.FirstOrDefault());
    }

    private void Validate(OrderDetailForShopQuery request)
    {
        var account = _currentAccountService.GetCurrentAccount();
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : _shopDeliveryStaffRepository.GetById(account.Id).ShopId;
        var order = _orderRepository
            .Get(o => o.Id == request.Id && o.ShopId == shopId).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);
    }
}