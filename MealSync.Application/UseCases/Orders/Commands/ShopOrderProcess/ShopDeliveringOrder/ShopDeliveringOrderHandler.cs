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

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopDeliveringOrder;

public class ShopDeliveringOrderHandler : ICommandHandler<ShopDeliveringOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShopDeliveringOrderCommand> _logger;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IShopRepository _shopRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;

    public ShopDeliveringOrderHandler(IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService, IUnitOfWork unitOfWork, ILogger<ShopDeliveringOrderCommand> logger,
        IDeliveryPackageRepository deliveryPackageRepository, INotificationFactory notificationFactory, INotifierService notifierService, IShopRepository shopRepository, IAccountRepository accountRepository,
        ISystemResourceRepository systemResourceRepository, IShopDeliveryStaffRepository shopDeliveryStaffRepository)
    {
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _deliveryPackageRepository = deliveryPackageRepository;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _shopRepository = shopRepository;
        _accountRepository = accountRepository;
        _systemResourceRepository = systemResourceRepository;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
    }

    public async Task<Result<Result>> Handle(ShopDeliveringOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        // Warning
        var order = _orderRepository.GetById(request.OrderId);
        if (!request.IsConfirm.Value)
        {
            var currentTime = TimeFrameUtils.GetCurrentDate();
            var currentTimeInMinutes = currentTime.Hour * 60 + currentTime.Minute;
            var startTimeInMinutes = TimeUtils.ConvertToMinutes(order.StartTime);
            if (startTimeInMinutes - currentTimeInMinutes > OrderConstant.TIME_WARNING_SHOP_PREPARE_ORDER_EARLY_IN_MINUTES)
            {
                var timeEarlyInHours = TimeFrameUtils.ConvertMinutesToHour(startTimeInMinutes - currentTimeInMinutes);
                return Result.Warning(new
                {
                    Code = MessageCode.W_ORDER_PREPARING_EARLY.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_PREPARING_EARLY.GetDescription(),
                        new string[] { order.Id.ToString(), TimeFrameUtils.GetTimeFrameString(order.StartTime, order.EndTime), TimeFrameUtils.GetTimeHoursFormat(timeEarlyInHours) }),
                });
            }
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            order.Status = OrderStatus.Delivering;

            // Get delivery package of shipper in this frame if not exist create
            await CreateOrAddOrderInDeliveryPackageAsync(order, request).ConfigureAwait(false);

            // Todo: Generate QR to order
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            // Noti to customer
            var shop = _shopRepository.GetById(order.ShopId);
            var notiCus = _notificationFactory.CreateOrderCustomerDeliveringNotification(order, shop);
            _notifierService.NotifyAsync(notiCus);

            // Noti to shop staff about delivery package
            if (request.ShipperId != null)
            {
                var accShip = _accountRepository.GetById(request.ShipperId.Value);
                var notiShopStaff = _notificationFactory.CreateOrderAssignedToStaffdNotification(order, accShip, shop);
                _notifierService.NotifyAsync(notiShopStaff);
            }

            return Result.Success(new
            {
                Code = MessageCode.I_ORDER_DELIVERING.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_DELIVERING.GetDescription(), order.Id),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task CreateOrAddOrderInDeliveryPackageAsync(Order order, ShopDeliveringOrderCommand request)
    {
        var shipId = request.ShipperId ?? _currentPrincipalService.CurrentPrincipalId.Value;
        var deliveryPackge = _deliveryPackageRepository.GetPackageByShipIdAndTimeFrame(request.ShipperId == null, shipId, order.StartTime, order.EndTime);
        if (deliveryPackge != null)
        {
            // Add order to current package
            deliveryPackge.Orders.Add(order);
            _deliveryPackageRepository.Update(deliveryPackge);
        }
        else
        {
            // Need create new delivery package
            var dp = new DeliveryPackage()
            {
                ShopDeliveryStaffId = request.ShipperId,
                ShopId = request.ShipperId == null ? _currentPrincipalService.CurrentPrincipalId.Value : null,
                DeliveryDate = order.IntendedReceiveDate,
                StartTime = order.StartTime,
                EndTime = order.EndTime,
                Status = DeliveryPackageStatus.Created,

            };
            dp.Orders.Add(order);
            await _deliveryPackageRepository.AddAsync(dp).ConfigureAwait(false);

            if (request.ShipperId != null)
            {
                var shopDeliveryStaff = _shopDeliveryStaffRepository.GetById(request.ShipperId.Value);
                shopDeliveryStaff.Status = ShopDeliveryStaffStatus.Busy;
                _shopDeliveryStaffRepository.Update(shopDeliveryStaff);
            }
        }

    }

    private void Validate(ShopDeliveringOrderCommand request)
    {
        var order = _orderRepository
            .Get(o => o.Id == request.OrderId && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
            .Include(o => o.DeliveryPackage).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId }, HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Preparing)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { request.OrderId });

        if (order.IntendedReceiveDate.Date != TimeFrameUtils.GetCurrentDate().Date)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_DELIVERING_IN_WRONG_DATE.GetDescription(), new object[] { order.Id, order.IntendedReceiveDate.Date.ToString("dd-MM-yyyy") });
    }
}