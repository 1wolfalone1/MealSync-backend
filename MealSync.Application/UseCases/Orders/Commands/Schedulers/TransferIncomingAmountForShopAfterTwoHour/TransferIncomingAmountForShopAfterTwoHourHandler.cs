using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.Schedulers.TransferIncomingAmountForShopAfterTwoHour;

public class TransferIncomingAmountForShopAfterTwoHourHandler : ICommandHandler<TransferIncomingAmountForShopAfterTwoHourCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransferIncomingAmountForShopAfterTwoHourHandler> _logger;
    private readonly INotifierService _notifierService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IBatchHistoryRepository _batchHistoryRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;

    public TransferIncomingAmountForShopAfterTwoHourHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, ILogger<TransferIncomingAmountForShopAfterTwoHourHandler> logger, INotifierService notifierService, INotificationFactory notificationFactory, IBatchHistoryRepository batchHistoryRepository, IWalletRepository walletRepository, IShopRepository shopRepository, IPaymentRepository paymentRepository, IWalletTransactionRepository walletTransactionRepository)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _notifierService = notifierService;
        _notificationFactory = notificationFactory;
        _batchHistoryRepository = batchHistoryRepository;
        _walletRepository = walletRepository;
        _shopRepository = shopRepository;
        _paymentRepository = paymentRepository;
        _walletTransactionRepository = walletTransactionRepository;
    }

    public async Task<Result<Result>> Handle(TransferIncomingAmountForShopAfterTwoHourCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var totalRecord = 0;
        var startTime = TimeFrameUtils.GetCurrentDate();
        var endTime = TimeFrameUtils.GetCurrentDate();

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var listOrderOverTimeFrame = _orderRepository.GetListOrderOnStatusFailDeliveredWithoutIncoming(OrderConstant.HOUR_ACCEPT_SHOP_FILL_REASON, TimeFrameUtils.GetCurrentDateInUTC7().DateTime);
            totalRecord = listOrderOverTimeFrame != null ? listOrderOverTimeFrame.Count : 0;
            var notifications = new List<Notification>();
            var ordersUpdate = new List<Order>();
            var orderGroupByShopId = listOrderOverTimeFrame.GroupBy(o => o.ShopId);
            foreach (var shopOrder in orderGroupByShopId)
            {
                var marlDeliveryFail = await TransferInComingAmountOfShopWallet(shopOrder.ToList(), shopOrder.Key).ConfigureAwait(false);
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
                BatchCode = BatchCodes.BatchCheduleTransferMoneyToShopWallet,
                Parameter = string.Empty,
                TotalRecord = totalRecord,
                ErrorLog = string.Join(", ", errors),
                StartDateTime = startTime,
                EndDateTime = endTime,
            };

            await _batchHistoryRepository.AddAsync(batchHistory).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Success(batchHistory);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task<(List<Order> Orders, List<Notification> Notifications)> TransferInComingAmountOfShopWallet(List<Order> orders, long shopId)
    {
        var notifications = new List<Notification>();
        foreach (var order in orders)
        {
            notifications.Add(await CreateTransaction(order).ConfigureAwait(false));
            order.IsPaidToShop = true;
        }

        return (orders, notifications);
    }

    private async Task<Notification> CreateTransaction(Order order)
    {
        var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
        var shop = _shopRepository.Get(sh => sh.Id == order.ShopId)
            .Include(sh => sh.Account)
            .Include(sh => sh.Wallet).Single();
        var payment = await _paymentRepository.GetPaymentVnPayByOrderId(order.Id).ConfigureAwait(false);

        var amountSendToShop = order.TotalPrice - order.TotalPromotion - order.ChargeFee;
        List<WalletTransaction> transactionsAdds = new();
        WalletTransaction transactionWithdrawalSystemTotalToShopWallet = new WalletTransaction
        {
            WalletFromId = systemTotalWallet.Id,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
            IncomingAmountBefore = systemTotalWallet.IncomingAmount,
            ReportingAmountBefore = systemTotalWallet.ReportingAmount,
            Amount = -amountSendToShop,
            Type = WalletTransactionType.Withdrawal,
            Description = $"Rút tiền từ ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(amountSendToShop)} VNĐ về ví cửa hàng id {order.ShopId} từ đơn hàng MS-{order.Id}",
        };
        transactionsAdds.Add(transactionWithdrawalSystemTotalToShopWallet);

        WalletTransaction transactionAddFromSystemTotalToShop = new WalletTransaction
        {
            WalletFromId = systemTotalWallet.Id,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shop.Wallet.AvailableAmount,
            IncomingAmountBefore = shop.Wallet.IncomingAmount,
            ReportingAmountBefore = shop.Wallet.ReportingAmount,
            Amount = amountSendToShop,
            Type = WalletTransactionType.Transfer,
            PaymentId = payment.Id,
            Description = $"Tiền thanh toán cho đơn hàng MS-{order.Id} {MoneyUtils.FormatMoneyWithDots(amountSendToShop)} VNĐ",
        };
        transactionsAdds.Add(transactionAddFromSystemTotalToShop);

        List<Wallet> wallets = new();
        systemTotalWallet.AvailableAmount -= amountSendToShop;
        shop.Wallet.IncomingAmount += amountSendToShop;
        wallets.Add(systemTotalWallet);
        wallets.Add(shop.Wallet);
        _walletRepository.UpdateRange(wallets);
        await _walletTransactionRepository.AddRangeAsync(transactionsAdds).ConfigureAwait(false);
        var noti = _notificationFactory.CreateShopWalletReceiveIncommingAmountNotification(order, shop.Account, amountSendToShop);
        return noti;
    }
}