using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reports.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Reports.Commands.CustomerReport;

public class CustomerReportHandler : ICommandHandler<CustomerReportCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReportRepository _reportRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IEmailService _emailService;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerReportHandler> _logger;
    private readonly IMapper _mapper;

    public CustomerReportHandler(
        IOrderRepository orderRepository, IReportRepository reportRepository,
        IPaymentRepository paymentRepository, IWalletRepository walletRepository,
        IShopRepository shopRepository, IAccountRepository accountRepository,
        IWalletTransactionRepository walletTransactionRepository, INotificationFactory notificationFactory,
        INotifierService notifierService, IEmailService emailService, ICurrentPrincipalService currentPrincipalService,
        IStorageService storageService, IUnitOfWork unitOfWork, ILogger<CustomerReportHandler> logger, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _reportRepository = reportRepository;
        _paymentRepository = paymentRepository;
        _walletRepository = walletRepository;
        _shopRepository = shopRepository;
        _accountRepository = accountRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _emailService = emailService;
        _currentPrincipalService = currentPrincipalService;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(CustomerReportCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;

        var order = await _orderRepository.GetByIdAndCustomerId(request.OrderId, customerId).ConfigureAwait(false);
        if (order == default)
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId });
        }
        else if (await _reportRepository.CheckExistedCustomerReport(request.OrderId, customerId).ConfigureAwait(false))
        {
            throw new InvalidBusinessException(MessageCode.E_REPORT_CUSTOMER_ALREADY_REPORT.GetDescription());
        }
        else if (order.Status != OrderStatus.Delivered && order.Status != OrderStatus.FailDelivery)
        {
            throw new InvalidBusinessException(MessageCode.E_REPORT_NOT_IN_STATUS_FOR_CUSTOMER_REPORT.GetDescription());
        }
        else
        {
            var now = DateTimeOffset.UtcNow;
            var receiveDateStartTime = new DateTime(
                order.IntendedReceiveDate.Year,
                order.IntendedReceiveDate.Month,
                order.IntendedReceiveDate.Day,
                order.StartTime / 100,
                order.StartTime % 100,
                0);
            DateTime receiveDateEndTime;
            if (order.EndTime == 2400)
            {
                receiveDateEndTime = new DateTime(
                        order.IntendedReceiveDate.Year,
                        order.IntendedReceiveDate.Month,
                        order.IntendedReceiveDate.Day,
                        0,
                        0,
                        0)
                    .AddDays(1);
            }
            else
            {
                receiveDateEndTime = new DateTime(
                    order.IntendedReceiveDate.Year,
                    order.IntendedReceiveDate.Month,
                    order.IntendedReceiveDate.Day,
                    order.EndTime / 100,
                    order.EndTime % 100,
                    0);
            }

            var startTime = new DateTimeOffset(receiveDateStartTime, TimeSpan.FromHours(7));
            var endTime = new DateTimeOffset(receiveDateEndTime, TimeSpan.FromHours(7));

            // Nếu sau 2h mới report mà shop đã nhận lỗi do nó thì ko cho report nữa
            // BR: Report within 12 hours after the 'endTime'
            if (now >= startTime && now <= endTime.AddHours(12) &&
                !(now >= endTime.AddHours(2)
                  && order.Status == OrderStatus.FailDelivery
                  && order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription()))
            {
                var imageUrls = new List<string>();
                if (request.Images != default && request.Images.Length > 0)
                {
                    foreach (var file in request.Images)
                    {
                        var url = await _storageService.UploadFileAsync(file).ConfigureAwait(false);
                        imageUrls.Add(url);
                    }
                }

                var report = new Report
                {
                    OrderId = request.OrderId,
                    CustomerId = customerId,
                    Title = request.Title,
                    Content = request.Content,
                    ImageUrl = imageUrls.Count > 0 ? string.Join(",", imageUrls) : string.Empty,
                    Status = ReportStatus.Pending,
                };

                var payment = await _paymentRepository.GetPaymentByOrderId(order.Id).ConfigureAwait(false);
                var shop = _shopRepository.GetById(order.ShopId)!;
                var shopWallet = _walletRepository.GetById(shop.WalletId)!;
                var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
                var isNotifyLimitAvailableAmount = false;

                try
                {
                    // Begin transaction
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                    if (order.Status == OrderStatus.FailDelivery)
                    {
                        if (payment.PaymentMethods == PaymentMethods.VnPay && payment.Status == PaymentStatus.PaidSuccess)
                        {
                            // Check cờ isPaidToShop cho trường hợp report trước 2h
                            if (order.IsPaidToShop)
                            {
                                // Tiền từ incoming về reporting
                                await TransactionInComingAmountToReportingAmount(payment, order, shop, shopWallet).ConfigureAwait(false);
                            }
                            else
                            {
                                // Tiền từ tổng hệ thống về reporting
                                await TransactionSystemAmountToReportingAmount(payment, order, systemTotalWallet, shop, shopWallet).ConfigureAwait(false);
                            }

                        }
                        else
                        {
                            // Do nothing
                        }
                    }
                    else
                    {
                        if (payment.PaymentMethods == PaymentMethods.VnPay && payment.Status == PaymentStatus.PaidSuccess)
                        {
                            // Tiền từ incoming về reporting
                            await TransactionInComingAmountToReportingAmount(payment, order, shop, shopWallet).ConfigureAwait(false);
                        }
                        else
                        {
                            // Tiền từ available về reporting
                            isNotifyLimitAvailableAmount = await TransactionAvailableAmountToReportingAmount(payment, order, shop, shopWallet).ConfigureAwait(false);
                        }
                    }

                    if (order.Status == OrderStatus.FailDelivery && order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription())
                    {
                        order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP_REPORTED_BY_CUSTOMER.GetDescription();
                    }
                    else if (order.Status == OrderStatus.FailDelivery && order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription())
                    {
                        order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription();
                    }
                    else
                    {
                        order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription();
                    }

                    order.IsReport = true;
                    order.Status = OrderStatus.IssueReported;
                    await _reportRepository.AddAsync(report).ConfigureAwait(false);
                    _orderRepository.Update(order);
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                    var notifications = new List<Notification>();

                    if (isNotifyLimitAvailableAmount)
                    {
                        var account = _accountRepository.GetById(shop.Id)!;
                        var notification = _notificationFactory.CreateLimitAvailableAmountAndInActiveShopNotification(shop, shopWallet);
                        notifications.Add(notification);
                        _emailService.SendNotifyLimitAvailableAmountAndInActiveShop(
                            account.Email,
                            MoneyUtils.FormatMoneyWithDots(shopWallet.AvailableAmount),
                            MoneyUtils.FormatMoneyWithDots(MoneyUtils.AVAILABLE_AMOUNT_LIMIT));
                    }

                    var accountCustomer = _accountRepository.GetById(customerId)!;
                    var customerReportOrderNotification = _notificationFactory.CreateCustomerReportOrderNotification(order, accountCustomer);
                    notifications.Add(customerReportOrderNotification);
                    _notifierService.NotifyRangeAsync(notifications);

                    return Result.Create(_mapper.Map<ReportDetailResponse>(report));
                }
                catch (Exception e)
                {
                    // Rollback when exception
                    _unitOfWork.RollbackTransaction();
                    _logger.LogError(e, e.Message);
                    throw new("Internal Server Error");
                }
            }
            else
            {
                throw new InvalidBusinessException(MessageCode.E_REPORT_CUSTOMER_REPORT_TIME_LIMIT.GetDescription());
            }
        }
    }

    private async Task TransactionSystemAmountToReportingAmount(Payment payment, Order order, Wallet systemTotalWallet, Shop shop, Wallet shopWallet)
    {

        var incomingAmountOrder = payment.Amount - order.ChargeFee;
        List<WalletTransaction> transactionsAdds = new();
        WalletTransaction transactionWithdrawalSystemTotalToShopWallet = new WalletTransaction
        {
            WalletFromId = systemTotalWallet.Id,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
            IncomingAmountBefore = systemTotalWallet.IncomingAmount,
            ReportingAmountBefore = systemTotalWallet.ReportingAmount,
            Amount = -incomingAmountOrder,
            PaymentId = payment.Id,
            Type = WalletTransactionType.Withdrawal,
            Description = $"Rút tiền từ ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(incomingAmountOrder)} VNĐ về ví chờ cửa hàng id {order.ShopId} từ đơn hàng MS-{order.Id}",
        };
        systemTotalWallet.AvailableAmount -= incomingAmountOrder;
        transactionsAdds.Add(transactionWithdrawalSystemTotalToShopWallet);

        WalletTransaction transactionAddFromSystemTotalToShop = new WalletTransaction
        {
            WalletFromId = systemTotalWallet.Id,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shop.Wallet.AvailableAmount,
            IncomingAmountBefore = shop.Wallet.IncomingAmount,
            ReportingAmountBefore = shop.Wallet.ReportingAmount,
            Amount = incomingAmountOrder,
            Type = WalletTransactionType.Transfer,
            PaymentId = payment.Id,
            Description = $"Tiền thanh toán cho đơn hàng MS-{order.Id} {MoneyUtils.FormatMoneyWithDots(incomingAmountOrder)} VNĐ về ví chờ",
        };
        shopWallet.IncomingAmount += incomingAmountOrder;
        transactionsAdds.Add(transactionAddFromSystemTotalToShop);

        var transactionWithdrawalIncomingAmountToReportingAmountOfShop = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shopWallet.AvailableAmount,
            IncomingAmountBefore = shopWallet.IncomingAmount,
            ReportingAmountBefore = shopWallet.ReportingAmount,
            Amount = -incomingAmountOrder,
            Type = WalletTransactionType.Withdrawal,
            Description = $"Rút tiền từ tiền chờ về {MoneyUtils.FormatMoneyWithDots(incomingAmountOrder)} VNĐ sang tiền đang bị báo cáo",
        };
        transactionsAdds.Add(transactionWithdrawalIncomingAmountToReportingAmountOfShop);
        shopWallet.IncomingAmount -= incomingAmountOrder;

        var transactionAddFromIncomingAmountToReportingAmountOfShop = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shopWallet.AvailableAmount,
            IncomingAmountBefore = shopWallet.IncomingAmount,
            ReportingAmountBefore = shopWallet.ReportingAmount,
            Amount = incomingAmountOrder,
            Type = WalletTransactionType.Transfer,
            Description = $"Tiền từ tiền chờ về cộng vào {MoneyUtils.FormatMoneyWithDots(incomingAmountOrder)} VNĐ tiền đang bị báo cáo",
        };

        shopWallet.ReportingAmount += incomingAmountOrder;
        transactionsAdds.Add(transactionAddFromIncomingAmountToReportingAmountOfShop);

        _walletRepository.Update(systemTotalWallet);
        _walletRepository.Update(shopWallet);
        await _walletTransactionRepository.AddRangeAsync(transactionsAdds).ConfigureAwait(false);
    }

    private async Task<bool> TransactionAvailableAmountToReportingAmount(Payment payment, Order order, Shop shop, Wallet shopWallet)
    {
        bool isNotify = false;
        var avaiableAmountOrder = payment.Amount - order.ChargeFee;
        var transactionWithdrawalIncomingAmountToReportingAmountOfShop = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shopWallet.AvailableAmount,
            IncomingAmountBefore = shopWallet.IncomingAmount,
            ReportingAmountBefore = shopWallet.ReportingAmount,
            Amount = -avaiableAmountOrder,
            Type = WalletTransactionType.Withdrawal,
            Description = $"Rút tiền từ tiền có sẵn {MoneyUtils.FormatMoneyWithDots(avaiableAmountOrder)} VNĐ sang tiền đang bị báo cáo",
        };

        shopWallet.AvailableAmount -= avaiableAmountOrder;

        var transactionAddFromIncomingAmountToReportingAmountOfShop = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shopWallet.AvailableAmount,
            IncomingAmountBefore = shopWallet.IncomingAmount,
            ReportingAmountBefore = shopWallet.ReportingAmount,
            Amount = avaiableAmountOrder,
            Type = WalletTransactionType.Transfer,
            Description = $"Tiền từ tiền có sẵn cộng vào {MoneyUtils.FormatMoneyWithDots(avaiableAmountOrder)} VNĐ tiền đang bị báo cáo",
        };

        shopWallet.ReportingAmount += avaiableAmountOrder;
        _walletRepository.Update(shopWallet);
        await _walletTransactionRepository.AddAsync(transactionWithdrawalIncomingAmountToReportingAmountOfShop).ConfigureAwait(false);
        await _walletTransactionRepository.AddAsync(transactionAddFromIncomingAmountToReportingAmountOfShop).ConfigureAwait(false);

        // BR: Tiền có sẵn < -200000 => shop inactive không được bán nữa cho tới khi nạp tiền vào
        if (shopWallet.AvailableAmount < MoneyUtils.AVAILABLE_AMOUNT_LIMIT)
        {
            isNotify = true;
            shop.Status = ShopStatus.InActive;
            _shopRepository.Update(shop);
        }

        return isNotify;
    }

    private async Task TransactionInComingAmountToReportingAmount(Payment payment, Order order, Shop shop, Wallet shopWallet)
    {

        var incomingAmountOrder = payment.Amount - order.ChargeFee;
        var transactionWithdrawalIncomingAmountToReportingAmountOfShop = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shopWallet.AvailableAmount,
            IncomingAmountBefore = shopWallet.IncomingAmount,
            ReportingAmountBefore = shopWallet.ReportingAmount,
            Amount = -incomingAmountOrder,
            Type = WalletTransactionType.Withdrawal,
            Description = $"Rút tiền từ tiền chờ về {MoneyUtils.FormatMoneyWithDots(incomingAmountOrder)} VNĐ sang tiền đang bị báo cáo",
        };

        shopWallet.IncomingAmount -= incomingAmountOrder;

        var transactionAddFromIncomingAmountToReportingAmountOfShop = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shopWallet.AvailableAmount,
            IncomingAmountBefore = shopWallet.IncomingAmount,
            ReportingAmountBefore = shopWallet.ReportingAmount,
            Amount = incomingAmountOrder,
            Type = WalletTransactionType.Transfer,
            Description = $"Tiền từ tiền chờ về cộng vào {MoneyUtils.FormatMoneyWithDots(incomingAmountOrder)} VNĐ tiền đang bị báo cáo",
        };

        shopWallet.ReportingAmount += incomingAmountOrder;
        _walletRepository.Update(shopWallet);
        await _walletTransactionRepository.AddAsync(transactionWithdrawalIncomingAmountToReportingAmountOfShop).ConfigureAwait(false);
        await _walletTransactionRepository.AddAsync(transactionAddFromIncomingAmountToReportingAmountOfShop).ConfigureAwait(false);
    }
}