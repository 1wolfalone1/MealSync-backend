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
        // else if (order.Status == OrderStatus.FailDelivery && order.ReasonIdentity != OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription())
        // {
        //     throw new InvalidBusinessException(MessageCode.E_REPORT_NOT_IN_STATUS_FOR_CUSTOMER_REPORT.GetDescription());
        // }
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

            // BR: Report within 12 hours after the 'endTime'
            if (now >= startTime && now <= endTime.AddHours(12))
            {
                var imageUrls = new List<string>();
                foreach (var file in request.Images)
                {
                    var url = await _storageService.UploadFileAsync(file).ConfigureAwait(false);
                    imageUrls.Add(url);
                }

                var report = new Report
                {
                    OrderId = request.OrderId,
                    CustomerId = customerId,
                    Title = request.Title,
                    Content = request.Content,
                    ImageUrl = string.Join(",", imageUrls),
                    Status = ReportStatus.Pending,
                };

                var payment = await _paymentRepository.GetPaymentByOrderId(order.Id).ConfigureAwait(false);
                var shop = _shopRepository.GetById(order.ShopId)!;
                var shopWallet = _walletRepository.GetById(shop.WalletId)!;
                var isNotifyLimitAvailableAmount = false;

                try
                {
                    // Begin transaction
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                    if (order.Status == OrderStatus.FailDelivery)
                    {
                        if (payment.PaymentMethods == PaymentMethods.VnPay && payment.Status == PaymentStatus.PaidSuccess)
                        {
                            // Tiền từ incoming về reporting
                            await TransactionInComingAmountToReportingAmount(payment, order, shop, shopWallet).ConfigureAwait(false);
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

                    order.IsReport = true;
                    order.ReasonIdentity = order.Status == OrderStatus.FailDelivery
                        ? OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_REPORTED_BY_CUSTOMER.GetDescription()
                        : OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription();
                    order.Status = OrderStatus.IssueReported;
                    await _reportRepository.AddAsync(report).ConfigureAwait(false);
                    _orderRepository.Update(order);
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                    if (isNotifyLimitAvailableAmount)
                    {
                        var account = _accountRepository.GetById(shop.Id)!;
                        var notification = _notificationFactory.CreateLimitAvailableAmountAndInActiveShopNotification(shop, shopWallet);
                        _notifierService.NotifyAsync(notification);
                        _emailService.SendNotifyLimitAvailableAmountAndInActiveShop(
                            account.Email,
                            MoneyUtils.FormatMoneyWithDots(shopWallet.AvailableAmount),
                            MoneyUtils.FormatMoneyWithDots(MoneyUtils.AVAILABLE_AMOUNT_LIMIT));
                    }

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