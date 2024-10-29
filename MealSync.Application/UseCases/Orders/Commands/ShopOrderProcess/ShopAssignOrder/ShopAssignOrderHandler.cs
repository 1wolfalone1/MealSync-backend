using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopDeliveringOrder;

public class ShopAssignOrderHandler : ICommandHandler<ShopAssignOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShopAssignOrderCommand> _logger;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IShopRepository _shopRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IMapper _mapper;

    public ShopAssignOrderHandler(IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService, IUnitOfWork unitOfWork, ILogger<ShopAssignOrderCommand> logger,
        IDeliveryPackageRepository deliveryPackageRepository, INotificationFactory notificationFactory, INotifierService notifierService, IShopRepository shopRepository, IAccountRepository accountRepository,
        ISystemResourceRepository systemResourceRepository, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IMapper mapper)
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
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(ShopAssignOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        // Warning
        var order = _orderRepository.GetById(request.OrderId);
        if (!request.IsConfirm.Value)
        {
            var now = TimeFrameUtils.GetCurrentDateInUTC7();
            var intendedReceiveDateTime = new DateTime(
                order.IntendedReceiveDate.Year,
                order.IntendedReceiveDate.Month,
                order.IntendedReceiveDate.Day,
                order.StartTime / 100,
                order.StartTime % 100,
                0);
            var endTime = new DateTimeOffset(intendedReceiveDateTime, TimeSpan.FromHours(7)).AddHours(-OrderConstant.TIME_WARNING_SHOP_ASSIGN_ORDER_EARLY_IN_HOURS);
            if (now < endTime)
            {
                var diffDate = endTime.AddHours(OrderConstant.TIME_WARNING_SHOP_ASSIGN_ORDER_EARLY_IN_HOURS) - now;
                return Result.Success(new
                {
                    Code = MessageCode.W_ORDER_ASSIGN_EARLY.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_ASSIGN_EARLY.GetDescription(),
                        new string[] { order.Id.ToString(), TimeFrameUtils.GetTimeFrameString(order.StartTime, order.EndTime), $"{diffDate.Hours}:{diffDate.Minutes}" }),
                });
            }
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            // Get delivery package of shipper in this frame if not exist create
            var dp = await CreateOrAddOrderInDeliveryPackageAsync(order, request).ConfigureAwait(false);

            // Todo: Generate QR to order
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            var listNoti = new List<Notification>();
            var shop = _shopRepository.GetById(order.ShopId);

            // Noti to shop staff about delivery package
            if (request.ShopDeliveryStaffId != null)
            {
                var accShip = _accountRepository.GetById(request.ShopDeliveryStaffId.Value);
                var notiShopStaff = _notificationFactory.CreateOrderAssignedToStaffNotification(dp, accShip, shop);
                listNoti.Add(notiShopStaff);
            }

            _notifierService.NotifyRangeAsync(listNoti);

            var deliveryPackageResponse = _deliveryPackageRepository.Get(deliveryPackage => deliveryPackage.Id == dp.Id)
                .Include(dp => dp.ShopDeliveryStaff)
                .ThenInclude(sds => sds.Account)
                .Include(dp => dp.Shop)
                .ThenInclude(sds => sds.Account).Single();
            return Result.Success(_mapper.Map<OrderDetailForShopResponse.ShopDeliveryStaffInShopOrderDetail>(deliveryPackageResponse));
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task<DeliveryPackage> CreateOrAddOrderInDeliveryPackageAsync(Order order, ShopAssignOrderCommand request)
    {
        var shipId = request.ShopDeliveryStaffId ?? _currentPrincipalService.CurrentPrincipalId.Value;
        var deliveryPackge = _deliveryPackageRepository.GetPackageByShipIdAndTimeFrame(request.ShopDeliveryStaffId == null, shipId, order.StartTime, order.EndTime);
        if (deliveryPackge != null)
        {
            // Add order to current package
            deliveryPackge.Orders.Add(order);
            _deliveryPackageRepository.Update(deliveryPackge);
            return deliveryPackge;
        }
        else
        {
            // Need create new delivery package
            var dp = new DeliveryPackage()
            {
                ShopDeliveryStaffId = request.ShopDeliveryStaffId,
                ShopId = request.ShopDeliveryStaffId == null ? _currentPrincipalService.CurrentPrincipalId.Value : null,
                DeliveryDate = order.IntendedReceiveDate,
                StartTime = order.StartTime,
                EndTime = order.EndTime,
                Status = DeliveryPackageStatus.Created,

            };
            dp.Orders.Add(order);
            await _deliveryPackageRepository.AddAsync(dp).ConfigureAwait(false);

            if (request.ShopDeliveryStaffId != null)
            {
                var shopDeliveryStaff = _shopDeliveryStaffRepository.GetById(request.ShopDeliveryStaffId.Value);
                shopDeliveryStaff.Status = ShopDeliveryStaffStatus.Busy;
                _shopDeliveryStaffRepository.Update(shopDeliveryStaff);
            }

            return dp;
        }
    }

    private void Validate(ShopAssignOrderCommand request)
    {
        var order = _orderRepository
            .Get(o => o.Id == request.OrderId && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
            .Include(o => o.DeliveryPackage).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId }, HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Preparing)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { request.OrderId });

        if (order.IntendedReceiveDate.Date != TimeFrameUtils.GetCurrentDateInUTC7().Date)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_DELIVERING_IN_WRONG_DATE.GetDescription(), new object[] { order.Id, order.IntendedReceiveDate.Date.ToString("dd-MM-yyyy") });

        if (order.DeliveryPackageId != null)
            throw new InvalidBusinessException(MessageCode.E_ORDER_IN_OTHER_PACKAGE.GetDescription(), new object[] { order.Id });

        if (request.ShopDeliveryStaffId != null)
        {
            var shopDeliveryStaff = _shopDeliveryStaffRepository.GetById(request.ShopDeliveryStaffId.Value);
            if (shopDeliveryStaff == null || shopDeliveryStaff.ShopId != _currentPrincipalService.CurrentPrincipalId)
                throw new InvalidBusinessException(MessageCode.E_ORDER_ASSIGN_NOT_FOUND_SHOP_STAFF.GetDescription(), new object[] { request.ShopDeliveryStaffId.Value });
        }
    }
}