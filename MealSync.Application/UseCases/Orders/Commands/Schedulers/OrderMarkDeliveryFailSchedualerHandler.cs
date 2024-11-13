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
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.Schedulers;

public class OrderMarkDeliveryFailSchedualerHandler : ICommandHandler<OrderMarkDeliveryFailSchedulerCommand, BatchHistory>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderMarkDeliveryFailSchedualerHandler> _logger;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IShopRepository _shopRepository;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IEmailService _emailService;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IBatchHistoryRepository _batchHistoryRepository;

    public OrderMarkDeliveryFailSchedualerHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, ILogger<OrderMarkDeliveryFailSchedualerHandler> logger, INotificationFactory notificationFactory, INotifierService notifierService, IShopRepository shopRepository, IDeliveryPackageRepository deliveryPackageRepository, IAccountRepository accountRepository, IBuildingRepository buildingRepository, IEmailService emailService, ISystemConfigRepository systemConfigRepository, IBatchHistoryRepository batchHistoryRepository)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _logger = logger;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _shopRepository = shopRepository;
        _deliveryPackageRepository = deliveryPackageRepository;
        _accountRepository = accountRepository;
        _buildingRepository = buildingRepository;
        _emailService = emailService;
        _systemConfigRepository = systemConfigRepository;
        _batchHistoryRepository = batchHistoryRepository;
    }

    public async Task<Result<BatchHistory>> Handle(OrderMarkDeliveryFailSchedulerCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var totalRecord = 0;
        var startTime = TimeFrameUtils.GetCurrentDate();
        var endTime = TimeFrameUtils.GetCurrentDate();
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var listOrderOverTimeFrame = _orderRepository.GetListOrderOnStatusDeliveringButOverTimeFrame(OrderConstant.HOUR_ACCEPT_SHOP_FILL_REASON, TimeFrameUtils.GetCurrentDateInUTC7().Date, TimeFrameUtils.GetCurrentHoursInUTC7());
            totalRecord = listOrderOverTimeFrame != null ? listOrderOverTimeFrame.Count : 0;
            var notifications = new List<Notification>();
            var ordersUpdate = new List<Order>();
            var orderGroupByShopId = listOrderOverTimeFrame.GroupBy(o => o.ShopId);
            foreach (var shopOrder in orderGroupByShopId)
            {
                var marlDeliveryFail = MarkShopDeliveryFail(shopOrder.Order().ToList(), shopOrder.Key);
                ordersUpdate.AddRange(marlDeliveryFail.Orders);
                notifications.AddRange(marlDeliveryFail.Notifications);
            }

            _notifierService.NotifyRangeAsync(notifications);
            _orderRepository.UpdateRange(ordersUpdate);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            endTime = TimeFrameUtils.GetCurrentDate();
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            errors.Add(e.Message);
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var batchHistory = new BatchHistory()
            {
                BatchCode = BatchCodes.BatchCheduleMarkDeliveryFail,
                Parameter = string.Empty,
                TotalRecord = totalRecord,
                ErrorLog = string.Join(", ", errors),
                StartDateTime = startTime,
                EndDateTime = endTime,
            };

            await _batchHistoryRepository.AddAsync(batchHistory).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return batchHistory;
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private (List<Order> Orders, List<Notification> Notifications) MarkShopDeliveryFail(List<Order> orders, long shopId)
    {
        var notfications = new List<Notification>();
        var numberOrderDeliveryFail = 0;
        foreach (var order in orders)
        {
            order.Status = OrderStatus.FailDelivery;
            order.Reason = "Cửa hàng không thực hiện việc giao hàng";
            order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription();
            numberOrderDeliveryFail++;
            notfications.AddRange(SendNotification(order));
        }

        // Mark flag to shop account
        var shopAccount = _accountRepository.GetById(shopId);
        shopAccount.NumOfFlag += numberOrderDeliveryFail;

        // Send email for shop annouce flag increase
        var systemConfig = _systemConfigRepository.GetSystemConfig();
        if (shopAccount.NumOfFlag >= systemConfig.MaxFlagsBeforeBan)
        {
            _emailService.SendEmailToAnnounceAccountGotBanned(shopAccount.Email, shopAccount.FullName);
            shopAccount.Status = AccountStatus.Banned;

        }
        else
        {
            var idOrders = string.Join(", ", orders.Select(o => string.Concat(IdPatternConstant.PREFIX_ID, o.Id)).ToList());
            _emailService.SendEmailToAnnounceApplyFlagForShop(shopAccount.Email, shopAccount.NumOfFlag, $"Bạn không thực hiện giao các đơn hàng sau:{idOrders}");
        }

        _accountRepository.Update(shopAccount);
        return (orders, notfications);
    }

    private List<Notification> SendNotification(Order order)
    {
        var notifications = new List<Notification>();
        var shop = _shopRepository.GetById(order.ShopId);
        // Send notification for customer
        var notiToCustomer = _notificationFactory.CreateOrderDeliveryFailedAutoByBatchToCustomerNotification(order, shop);
        notifications.Add(notiToCustomer);

        // Send notification for shop
        var notiToShop = _notificationFactory.CreateOrderDeliveryFailedAutoByBatchToShopNotification(order, shop);
        notifications.Add(notiToShop);

        // Send notification for moderator
        var building = _buildingRepository.GetById(order.BuildingId);
        var moderators = _accountRepository.GetAccountsOfModeratorByDormitoryId(building.DormitoryId);
        foreach (var moderator in moderators)
        {
            var notiToModerator = _notificationFactory.CreateOrderDeliveryFailedToModeratorNotification(order, moderator, shop);
            notifications.Add(notiToModerator);
        }

        return notifications;
    }
}