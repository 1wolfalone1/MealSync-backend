using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Shops.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Shops.Queries.ShopInfoForReOrder;

public class ShopInfoForReOrderHandler : IQueryHandler<ShopInfoForReOrderQuery, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public ShopInfoForReOrderHandler(
        IOrderRepository orderRepository, IShopRepository shopRepository,
        ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(ShopInfoForReOrderQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var order = _orderRepository.Get(o => o.Id == request.OrderId && o.CustomerId == customerId).FirstOrDefault();
        var shopInfoReOrderResponse = new ShopInfoReOrderResponse();
        if (order == default)
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId });
        }
        else
        {
            var shopInfo = await _shopRepository.GetShopInfoForReOrderById(order.ShopId).ConfigureAwait(false);
            if (shopInfo.Status != ShopStatus.Active)
            {
                shopInfoReOrderResponse.IsAllowReOrder = false;
                shopInfoReOrderResponse.MessageNotAllow = "Cửa hàng đã đóng cửa, bạn không thể đặt lại đơn hàng vào bây giờ.";
                return Result.Success(shopInfoReOrderResponse);
            }
            else
            {
                shopInfoReOrderResponse.IsAllowReOrder = true;
                shopInfoReOrderResponse.MessageNotAllow = default;
                var operatingSlotReOrderResponses = new List<ShopInfoReOrderResponse.ShopOperatingSlotReOrderResponse>();
                foreach (var operatingSlot in shopInfo.OperatingSlots)
                {
                    var shopOperatingSlotReOrderResponse = new ShopInfoReOrderResponse.ShopOperatingSlotReOrderResponse
                    {
                        Id = operatingSlot.Id,
                        Title = operatingSlot.Title,
                        StartTime = operatingSlot.StartTime,
                        EndTime = operatingSlot.EndTime,
                        IsAcceptingOrderTomorrow = shopInfo.IsAcceptingOrderNextDay,
                        IsAcceptingOrderToday = !shopInfo.IsReceivingOrderPaused && !operatingSlot.IsReceivingOrderPaused,
                    };
                    operatingSlotReOrderResponses.Add(shopOperatingSlotReOrderResponse);
                }

                shopInfoReOrderResponse.Dormitories = _mapper.Map<List<ShopInfoReOrderResponse.ShopDormitoryReOrderResponse>>(shopInfo.ShopDormitories.Select(sd => sd.Dormitory));
                shopInfoReOrderResponse.OperatingSlots = operatingSlotReOrderResponses;

                return Result.Success(shopInfoReOrderResponse);
            }
        }
    }
}