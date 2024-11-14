using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopDeliveryFailOrder;

public class ShopDeliveryFailOrderHandler : ICommandHandler<ShopDeliveryFailOrderCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<ShopDeliveryFailOrderHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IShopRepository _shopRepository;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public ShopDeliveryFailOrderHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, ILogger<ShopDeliveryFailOrderHandler> logger, ICurrentPrincipalService currentPrincipalService, INotificationFactory notificationFactory, INotifierService notifierService, IShopRepository shopRepository, IDeliveryPackageRepository deliveryPackageRepository, IAccountRepository accountRepository, IBuildingRepository buildingRepository, ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _shopRepository = shopRepository;
        _deliveryPackageRepository = deliveryPackageRepository;
        _accountRepository = accountRepository;
        _buildingRepository = buildingRepository;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(ShopDeliveryFailOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var order = _orderRepository.GetById(request.OrderId);
            order.Status = OrderStatus.FailDelivery;
            order.Reason = request.Reason;
            order.LastestDeliveryFailAt = TimeFrameUtils.GetCurrentDate();

            if (request.Evidences != null && request.Evidences.Count > 0)
            {
                order.EvidenceDeliveryFailJson = JsonConvert.SerializeObject(request.Evidences);
            }

            if (request.ReasonIndentity == 1)
            {
                order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription();
            }
            else
            {
                order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription();
            }

            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            // Send notification
            SendNotification(order);

            return Result.Success(new
            {
                Code = MessageCode.I_ORDER_DELIVERY_FAIL.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_DELIVERY_FAIL.GetDescription(), order.Id),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(ShopDeliveryFailOrderCommand request)
    {
        var order = _orderRepository
            .Get(o => o.Id == request.OrderId && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
            .Include(o => o.DeliveryPackage).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId }, HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Delivering && order.Status != OrderStatus.FailDelivery)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { request.OrderId });

        // Check is > Start time and less than end time + 2 hours
        var currentDateTime = TimeFrameUtils.GetCurrentDateInUTC7();
        var startEndDateTime = TimeFrameUtils.GetStartTimeEndTimeToDateTime(order.IntendedReceiveDate, order.StartTime, order.EndTime);
        var endDateTime = startEndDateTime.EndTime.AddHours(OrderConstant.HOUR_ACCEPT_SHOP_FILL_REASON);
        if (currentDateTime.DateTime < startEndDateTime.StartTime || currentDateTime.DateTime > endDateTime)
            throw new InvalidBusinessException(MessageCode.E_ORDER_DELIVERY_FAIL_OVER_TIME_FILL_REASON.GetDescription(), new object[] { request.OrderId });
    }

    private async Task SendNotification(Order order)
    {
        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
        // Send notification for customer
        var notiToCustomer = _notificationFactory.CreateOrderDeliveryFailedToCustomerNotification(order, shop);
        await _notifierService.NotifyAsync(notiToCustomer).ConfigureAwait(false);

        // Send notification for shop
        var deliveryPackage = _deliveryPackageRepository.GetById(order.DeliveryPackageId.Value);
        var accShip = _accountRepository.GetById(deliveryPackage.ShopDeliveryStaffId ?? deliveryPackage.ShopId);
        var notiToShop = _notificationFactory.CreateOrderDeliveryFailedToShopNotification(order, accShip, shop);
        await _notifierService.NotifyAsync(notiToShop).ConfigureAwait(false);

        // Send notification for moderator
        var building = _buildingRepository.GetById(order.BuildingId);
        var moderators = _accountRepository.GetAccountsOfModeratorByDormitoryId(building.DormitoryId);
        foreach (var moderator in moderators)
        {
            var notiToModerator = _notificationFactory.CreateOrderDeliveryFailedToModeratorNotification(order, moderator, shop);
            await _notifierService.NotifyAsync(notiToModerator).ConfigureAwait(false);
        }
    }
}