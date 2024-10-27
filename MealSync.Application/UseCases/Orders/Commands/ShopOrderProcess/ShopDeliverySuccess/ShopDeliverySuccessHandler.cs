using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopDeliverySuccess;

public class ShopDeliverySuccessHandler : ICommandHandler<ShopDeliverySuccessCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<ShopDeliverySuccessHandler> _logger;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IAccountRepository _accountRepository;

    public ShopDeliverySuccessHandler(IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService, IOrderRepository orderRepository, ILogger<ShopDeliverySuccessHandler> logger,
        ISystemResourceRepository systemResourceRepository, IShopRepository shopRepository, IOrderDetailRepository orderDetailRepository, IFoodRepository foodRepository, INotificationFactory notificationFactory,
        INotifierService notifierService, IAccountRepository accountRepository)
    {
        _unitOfWork = unitOfWork;
        _currentPrincipalService = currentPrincipalService;
        _orderRepository = orderRepository;
        _logger = logger;
        _systemResourceRepository = systemResourceRepository;
        _shopRepository = shopRepository;
        _orderDetailRepository = orderDetailRepository;
        _foodRepository = foodRepository;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _accountRepository = accountRepository;
    }

    public async Task<Result<Result>> Handle(ShopDeliverySuccessCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var order = _orderRepository.Get(o => o.Id == request.OrderId)
                .Include(o => o.DeliveryPackage).Single();
            order.Status = OrderStatus.Delivered;

            // Increase food total order
            var listFoodIds = _orderDetailRepository.GetListFoodIdInOrderDetailGroupBy(request.OrderId);
            var listFood = _foodRepository.Get(f => listFoodIds.Contains(f.Id)).ToList();
            foreach (var food in listFood)
            {
                food.TotalOrder++;
            }

            // Increase shop total order
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
            shop.TotalOrder++;

            _foodRepository.UpdateRange(listFood);
            _orderRepository.Update(order);
            _shopRepository.Update(shop);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            // Noti to customer
            var notiCustomer = _notificationFactory.CreateOrderCustomerDeliveredNotification(order, shop);
            _notifierService.NotifyAsync(notiCustomer);

            // Noti to shop
            Account accShip;
            if (order.DeliveryPackage.ShopDeliveryStaffId != null)
            {
                accShip = _accountRepository.GetById(order.DeliveryPackage.ShopDeliveryStaffId);
            }
            else
            {
                accShip = _accountRepository.GetById(order.ShopId);
            }

            var notiShop = _notificationFactory.CreateOrderShopDeliveredNotification(order, accShip);
            _notifierService.NotifyAsync(notiShop);
            return Result.Success(new
            {
                Code = MessageCode.I_ORDER_DELIVERD.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_DELIVERD.GetDescription(), new object[] { request.OrderId }),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(ShopDeliverySuccessCommand request)
    {
        var order = _orderRepository
            .Get(o => o.Id == request.OrderId && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
            .Include(o => o.DeliveryPackage).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId }, HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Delivering)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { request.OrderId });

        if (order.DeliveryPackage != null)
        {
            if (order.CustomerId != request.CustomerId)
            {
                // Đơn hàng không phải của khách hàng
                throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_CORRECT_CUSTOMER.GetDescription(), new object[] { request.OrderId, order.FullName });
            }

            if (order.DeliveryPackage.ShopDeliveryStaffId != null && order.DeliveryPackage.ShopDeliveryStaffId == request.ShipperId)
            {
                // Đơn hàng này không phải do bạn giao
            }

            if (order.DeliveryPackage.ShopId != null && order.DeliveryPackage.ShopId == request.ShipperId)
            {
                // Đơn hàng này không phải do bạn giao
            }
        }

        // Todo: Check token gen is correct
    }
}