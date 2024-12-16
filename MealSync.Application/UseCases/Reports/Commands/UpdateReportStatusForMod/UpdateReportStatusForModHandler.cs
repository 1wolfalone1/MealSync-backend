using System.Text.Json;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Services.Payments.VnPay;
using MealSync.Application.Common.Services.Payments.VnPay.Models;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Reports.Commands.UpdateReportStatusForMod;

public class UpdateReportStatusForModHandler : ICommandHandler<UpdateReportStatusForModCommand, Result>
{
    private readonly IReportRepository _reportRepository;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IOrderRepository _orderRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IVnPayPaymentService _vnPayPaymentService;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IAccountFlagRepository _accountFlagRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateReportStatusForModHandler> _logger;

    public UpdateReportStatusForModHandler(
        IReportRepository reportRepository, IModeratorDormitoryRepository moderatorDormitoryRepository,
        ICurrentPrincipalService currentPrincipalService, IOrderRepository orderRepository,
        IShopRepository shopRepository, IWalletRepository walletRepository, IAccountRepository accountRepository,
        ISystemConfigRepository systemConfigRepository, ISystemResourceRepository systemResourceRepository,
        IVnPayPaymentService vnPayPaymentService, IBuildingRepository buildingRepository,
        IWalletTransactionRepository walletTransactionRepository, INotificationFactory notificationFactory,
        INotifierService notifierService, IPaymentRepository paymentRepository, IAccountFlagRepository accountFlagRepository,
        ICustomerRepository customerRepository, IEmailService emailService, IUnitOfWork unitOfWork, ILogger<UpdateReportStatusForModHandler> logger)
    {
        _reportRepository = reportRepository;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _currentPrincipalService = currentPrincipalService;
        _orderRepository = orderRepository;
        _shopRepository = shopRepository;
        _walletRepository = walletRepository;
        _accountRepository = accountRepository;
        _systemConfigRepository = systemConfigRepository;
        _systemResourceRepository = systemResourceRepository;
        _vnPayPaymentService = vnPayPaymentService;
        _buildingRepository = buildingRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _paymentRepository = paymentRepository;
        _accountFlagRepository = accountFlagRepository;
        _customerRepository = customerRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(UpdateReportStatusForModCommand request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();

        var orderId = await _reportRepository.GetOrderIdByCustomerReportIdAndDormitoryIds(request.Id, dormitoryIds).ConfigureAwait(false);

        if (orderId == default || orderId == 0)
        {
            throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
        }
        else
        {
            var order = await _orderRepository.GetOrderIncludePaymentById(orderId.Value).ConfigureAwait(false);
            var now = TimeFrameUtils.GetCurrentDateInUTC7();
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

            var endTime = new DateTimeOffset(receiveDateEndTime, TimeSpan.FromHours(7));

            var reports = await _reportRepository.GetByOrderId(orderId.Value).ConfigureAwait(false);
            var customerReport = reports.First(r => r.CustomerId != default);
            var shopReport = reports.Count > 1 ? reports.First(r => r.ShopId != default) : default;
            var shopAccount = _accountRepository.GetById(order.ShopId)!;
            var customer = await _customerRepository.GetIncludeAccount(order.CustomerId).ConfigureAwait(false);
            var shop = _shopRepository.GetById(order.ShopId)!;

            // BR: Mod report after shop reply report or now > time report limit(20h)
            if (customerReport.Status != ReportStatus.Pending)
            {
                throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
            }
            else if (
                order.Status == OrderStatus.IssueReported
                && (order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription() || order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription())
                && customerReport.Status == ReportStatus.Pending
                && request.Status == UpdateReportStatusForModCommand.ProcessReportStatus.UnderReview
                && ((reports.Count > 1 && (order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription() || now > endTime.AddHours(2))) || now > endTime.AddHours(20))
            )
            {
                try
                {
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                    order.Status = OrderStatus.UnderReview;
                    _orderRepository.Update(order);
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                    NotifyUnderReview(customer.Account, shop, customerReport);
                    return Result.Success(new
                    {
                        Code = MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription(),
                        Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription()),
                    });
                }
                catch (Exception e)
                {
                    _unitOfWork.RollbackTransaction();
                    _logger.LogError(e, e.Message);
                    throw new("Internal Server Error");
                }
            }
            else if (
                order.Status == OrderStatus.UnderReview
                && (order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription() || order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription())
                && customerReport.Status == ReportStatus.Pending
                && (
                    request.Status == UpdateReportStatusForModCommand.ProcessReportStatus.Approved
                    || request.Status == UpdateReportStatusForModCommand.ProcessReportStatus.Rejected)
                && ((reports.Count > 1 && (order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription() || now > endTime.AddHours(2))) || now > endTime.AddHours(20))
            )
            {
                var payment = order.Payments.First(p => p.Type == PaymentTypes.Payment);
                var systemConfig = _systemConfigRepository.GetSystemConfig();
                var shopWallet = _walletRepository.GetById(shop.WalletId)!;

                if (request.Status == UpdateReportStatusForModCommand.ProcessReportStatus.Approved)
                {
                    try
                    {
                        await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                        if (order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription())
                        {
                            // Fail delivery
                            if (payment.PaymentMethods == PaymentMethods.COD)
                            {
                                // Giao hàng thất bại, thanh toán COD => Approve customer report => Đánh cờ shop
                                await ApproveCustomerAndFlagShop(request, customerReport, shopReport, shop, systemConfig, shopAccount).ConfigureAwait(false);
                            }
                            else
                            {
                                // Giao hàng thất bại, thanh toán Online => Approve customer report, đánh cờ shop => Refund tiền customer
                                await ApproveCustomerAndFlagShop(request, customerReport, shopReport, shop, systemConfig, shopAccount).ConfigureAwait(false);
                                await TransactionWithdrawalReportingForRefund(payment, order, shop, shopWallet).ConfigureAwait(false);
                                order.IsRefund = await RefundOrderAsync(order, payment).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            // Delivered
                            if (payment.PaymentMethods == PaymentMethods.COD)
                            {
                                // Giao hàng thành công, thanh toán COD => Approve customer report, đánh cờ shop => Refund bằng tiền sàn, chuyển lại tiền từ reporting về available (Payment - ChargeFee)
                                await ApproveCustomerAndFlagShop(request, customerReport, shopReport, shop, systemConfig, shopAccount).ConfigureAwait(false);
                                await TransactionReportingToAvailable(payment, order, shop, shopWallet).ConfigureAwait(false);
                            }
                            else
                            {
                                // Giao hàng thành công, thanh toán Online => Approve customer report, đánh cờ shop => Refund bằng tiền sàn, chuyển lại tiền từ reporting về available (Payment - ChargeFee)
                                await ApproveCustomerAndFlagShop(request, customerReport, shopReport, shop, systemConfig, shopAccount).ConfigureAwait(false);
                                await TransactionReportingToAvailable(payment, order, shop, shopWallet).ConfigureAwait(false);
                            }
                        }

                        // Resolved Order
                        order.Status = OrderStatus.Resolved;
                        _orderRepository.Update(order);

                        await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                        NotifyApproveOrReject(customer.Account, shop, customerReport);
                        return Result.Success(new
                        {
                            Code = MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription(),
                            Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription()),
                        });
                    }
                    catch (Exception e)
                    {
                        _unitOfWork.RollbackTransaction();
                        _logger.LogError(e, e.Message);
                        throw new("Internal Server Error");
                    }
                }
                else if (request.Status == UpdateReportStatusForModCommand.ProcessReportStatus.Rejected)
                {
                    var isFlagCustomer = false;
                    try
                    {
                        await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                        if (order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription())
                        {
                            // Fail delivery
                            if (payment.PaymentMethods == PaymentMethods.COD)
                            {
                                // Giao hàng thất bại, thanh toán COD => Reject customer report, đánh cờ cus
                                RejectCustomerReport(request, customerReport, shopReport);
                                await FlagCustomerAccount(customer, order, systemConfig).ConfigureAwait(false);
                                isFlagCustomer = true;
                            }
                            else
                            {
                                // Giao hàng thất bại, thanh toán Online => Reject customer report => Tiền từ ví reporting về ví available (Payment - ChargeFee)
                                RejectCustomerReport(request, customerReport, shopReport);
                                await TransactionReportingToAvailable(payment, order, shop, shopWallet).ConfigureAwait(false);
                                isFlagCustomer = true;
                            }
                        }
                        else
                        {
                            // Delivered
                            if (payment.PaymentMethods == PaymentMethods.COD)
                            {
                                // Giao hàng thành công, thanh toán COD => Reject customer report => Tiền từ ví reporting về ví available (Payment - ChargeFee)
                                RejectCustomerReport(request, customerReport, shopReport);
                                await TransactionReportingToAvailable(payment, order, shop, shopWallet).ConfigureAwait(false);
                            }
                            else
                            {
                                // Giao hàng thành công, thanh toán Online => Reject customer report => Tiền từ ví reporting về ví available (Payment - ChargeFee)
                                RejectCustomerReport(request, customerReport, shopReport);
                                await TransactionReportingToAvailable(payment, order, shop, shopWallet).ConfigureAwait(false);
                            }
                        }

                        // Resolved Order
                        order.Status = OrderStatus.Resolved;
                        _orderRepository.Update(order);

                        await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                        if (isFlagCustomer)
                        {
                            SendMailBanCustomer(customer, systemConfig);
                        }

                        NotifyApproveOrReject(customer.Account, shop, customerReport);

                        return Result.Success(new
                        {
                            Code = MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription(),
                            Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription()),
                        });
                    }
                    catch (Exception e)
                    {
                        _unitOfWork.RollbackTransaction();
                        _logger.LogError(e, e.Message);
                        throw new("Internal Server Error");
                    }
                }
                else
                {
                    throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
                }
            }
            else
            {
                throw new InvalidBusinessException(MessageCode.E_MODERATOR_NOT_YET_PROCESSED_REPORT.GetDescription());
            }
        }
    }

    private void NotifyApproveOrReject(Account customerAccount, Shop shop, Report customerReport)
    {
        if (customerReport.Status == ReportStatus.Approved)
        {
            var customerNotify = _notificationFactory.CreateApproveOrRejectCustomerReportNotification(customerAccount, shop, customerReport, true);
            var shopNotify = _notificationFactory.CreateApproveOrRejectReportOfShopNotification(shop, customerAccount, customerReport, false);
            _notifierService.NotifyAsync(customerNotify);
            _notifierService.NotifyAsync(shopNotify);
        }
        else
        {
            var customerNotify = _notificationFactory.CreateApproveOrRejectCustomerReportNotification(customerAccount, shop, customerReport, false);
            var shopNotify = _notificationFactory.CreateApproveOrRejectReportOfShopNotification(shop, customerAccount, customerReport, true);
            _notifierService.NotifyAsync(customerNotify);
            _notifierService.NotifyAsync(shopNotify);
        }
    }

    private void NotifyUnderReview(Account customerAccount, Shop shop, Report customerReport)
    {
        var customerNotify = _notificationFactory.CreateUnderReviewCustomerReportNotification(customerAccount, shop, customerReport);
        var shopNotify = _notificationFactory.CreateUnderReviewReportOfShopNotification(shop, customerAccount, customerReport);
        _notifierService.NotifyAsync(customerNotify);
        _notifierService.NotifyAsync(shopNotify);
    }

    private void SendMailBanShop(Account account)
    {

    }

    private void SendMailBanCustomer(Customer customer, SystemConfig systemConfig)
    {
        if (customer.Account.NumOfFlag < systemConfig.MaxFlagsBeforeBan)
        {
            var notification = _notificationFactory.CreateWarningFlagCustomerNotification(customer.Account);
            _notifierService.NotifyAsync(notification);
        }
        else
        {
            _emailService.SendNotifyBannedCustomerAccount(customer.Account.Email, customer.Account.FullName, customer.Account.NumOfFlag);
        }
    }

    private async Task FlagCustomerAccount(Customer customer, Order order, SystemConfig systemConfig)
    {

        var accountFlag = GetAccountFlagForFailDeliveryByCustomer(customer.Account, order);
        customer.Account.NumOfFlag += 1;

        if (customer.Account.NumOfFlag >= systemConfig.MaxFlagsBeforeBan)
        {
            var totalOrderInProcess = await _orderRepository.CountTotalOrderInProcessByCustomerId(customer.Id).ConfigureAwait(false);
            var ordersCancelBeforeBan = await _orderRepository.GetForSystemCancelByCustomerId(customer.Id).ConfigureAwait(false);

            if (totalOrderInProcess > 0)
            {
                customer.Status = CustomerStatus.Banning;
            }
            else
            {
                customer.Status = CustomerStatus.Banned;
                customer.Account.Status = AccountStatus.Banned;
            }

            await CancelOrderPendingOrConfirmedForBanCustomer(ordersCancelBeforeBan).ConfigureAwait(false);
        }

        _customerRepository.Update(customer);
        await _accountFlagRepository.AddAsync(accountFlag).ConfigureAwait(false);
    }

    private void RejectCustomerReport(UpdateReportStatusForModCommand request, Report customerReport, Report? shopReport)
    {

        customerReport.Status = ReportStatus.Rejected;
        customerReport.Reason = request.Reason;
        _reportRepository.Update(customerReport);
        if (shopReport != default)
        {
            shopReport.Status = ReportStatus.Approved;
            shopReport.Reason = request.Reason;
            _reportRepository.Update(shopReport);
        }
    }

    private AccountFlag GetAccountFlagForFailDeliveryByCustomer(Account customerAccount, Order order)
    {
        return new AccountFlag
        {
            AccountId = customerAccount.Id,
            ActionType = AccountActionTypes.FailDeliveryByCustomerOrder,
            TargetType = AccountTargetTypes.Order,
            TargetId = order.Id.ToString(),
            Description = $"Khách hàng không nhận hàng giao từ cửa hàng đơn hàng MS-{order.Id}",
        };
    }

    private async Task TransactionWithdrawalReportingForRefund(Payment payment, Order order, Shop shop, Wallet shopWallet)
    {
        var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
        var reportAmountOrder = payment.Amount - order.ChargeFee;
        var transactions = new List<WalletTransaction>();

        var transactionWithdrawalReportingForRefund = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = systemTotalWallet.Id,
            AvaiableAmountBefore = shopWallet.AvailableAmount,
            IncomingAmountBefore = shopWallet.IncomingAmount,
            ReportingAmountBefore = shopWallet.ReportingAmount,
            Amount = -reportAmountOrder,
            Type = WalletTransactionType.Withdrawal,
            Description = $"Rút tiền từ tiền đang bị báo cáo {MoneyUtils.FormatMoneyWithDots(reportAmountOrder)} VNĐ về ví tổng hệ thống để hoàn tiền cho khách hàng đơn hàng MS-{order.Id}",
        };
        transactions.Add(transactionWithdrawalReportingForRefund);
        shopWallet.ReportingAmount -= reportAmountOrder;

        var transactionTransferReportingForRefund = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = systemTotalWallet.Id,
            AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
            IncomingAmountBefore = systemTotalWallet.IncomingAmount,
            ReportingAmountBefore = systemTotalWallet.ReportingAmount,
            Amount = -reportAmountOrder,
            Type = WalletTransactionType.Transfer,
            Description = $"Tiền từ tiền đang bị báo cáo {MoneyUtils.FormatMoneyWithDots(reportAmountOrder)} VNĐ về ví tổng hệ thống để hoàn tiền cho khách hàng đơn hàng MS-{order.Id}",
        };
        transactions.Add(transactionTransferReportingForRefund);
        systemTotalWallet.AvailableAmount += reportAmountOrder;

        await _walletTransactionRepository.AddRangeAsync(transactions).ConfigureAwait(false);
        _walletRepository.Update(shopWallet);
        _walletRepository.Update(systemTotalWallet);
    }

    private async Task TransactionReportingToAvailable(Payment payment, Order order, Shop shop, Wallet shopWallet)
    {

        // Reporting to available
        var walletTransactions = new List<WalletTransaction>();
        var reportAmountOrder = payment.Amount - order.ChargeFee;
        var transactionWithdrawalReportingAmountToAvailableAmountOfShop = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shopWallet.AvailableAmount,
            IncomingAmountBefore = shopWallet.IncomingAmount,
            ReportingAmountBefore = shopWallet.ReportingAmount,
            Amount = -reportAmountOrder,
            Type = WalletTransactionType.Withdrawal,
            Description = $"Rút tiền từ tiền đang bị báo cáo {MoneyUtils.FormatMoneyWithDots(reportAmountOrder)} VNĐ sang tiền có sẵn",
        };

        shopWallet.ReportingAmount -= reportAmountOrder;

        var transactionAddFromReportingAmountToAvailableAmountOfShop = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shopWallet.AvailableAmount,
            IncomingAmountBefore = shopWallet.IncomingAmount,
            ReportingAmountBefore = shopWallet.ReportingAmount,
            Amount = reportAmountOrder,
            Type = WalletTransactionType.Transfer,
            Description = $"Tiền từ tiền đang bị báo cáo vào {MoneyUtils.FormatMoneyWithDots(reportAmountOrder)} VNĐ tiền có sẵn",
        };

        shopWallet.AvailableAmount += reportAmountOrder;

        walletTransactions.Add(transactionWithdrawalReportingAmountToAvailableAmountOfShop);
        walletTransactions.Add(transactionAddFromReportingAmountToAvailableAmountOfShop);

        await _walletTransactionRepository.AddRangeAsync(walletTransactions).ConfigureAwait(false);
        _walletRepository.Update(shopWallet);
    }

    private async Task ApproveCustomerAndFlagShop(UpdateReportStatusForModCommand request, Report customerReport, Report? shopReport, Shop shop, SystemConfig systemConfig, Account shopAccount)
    {
        customerReport.Status = ReportStatus.Approved;
        customerReport.Reason = request.Reason;
        _reportRepository.Update(customerReport);
        if (shopReport != default)
        {
            shopReport.Status = ReportStatus.Rejected;
            shopReport.Reason = request.Reason;
            _reportRepository.Update(shopReport);
        }

        shop.NumOfWarning += 1;

        if (shop.NumOfWarning >= 3 && shop.NumOfWarning < systemConfig.MaxWarningBeforeInscreaseFlag)
        {
            // Cờ cảnh báo thứ 3 hoặc 4 sẽ send mail thông báo
        }
        else if (shop.NumOfWarning >= systemConfig.MaxWarningBeforeInscreaseFlag)
        {
            // Cờ cảnh cáo thứ 5 => tăng cờ ở account, reset cờ cảnh báo
            shopAccount.NumOfFlag += 1;
            shop.NumOfWarning = 0;
            if (shopAccount.NumOfFlag < systemConfig.MaxFlagsBeforeBan)
            {
                // Cờ ở account < 3 => email notify warning
            }
            else
            {
                // Cờ ở account = 3 => email notify ban + ban account(Check banning, banned)
                var totalOrderInProcess = await _orderRepository.CountTotalOrderInProcessByShopId(shop.Id).ConfigureAwait(false);
                if (totalOrderInProcess > 0)
                {
                    shop.Status = ShopStatus.Banning;
                }
                else
                {
                    shop.Status = ShopStatus.Banned;
                    shopAccount.Status = AccountStatus.Banned;
                }

                var ordersCancel = await _orderRepository.GetForSystemCancelByShopId(shop.Id).ConfigureAwait(false);
                await CancelOrderPendingOrConfirmedForBanShop(ordersCancel).ConfigureAwait(false);

                // Update account flag and status
                _accountRepository.Update(shopAccount);
            }
        }

        // Update shop warning and status
        _shopRepository.Update(shop);
    }

    private async Task CancelOrderPendingOrConfirmedForBanCustomer(List<Order> ordersCancelBeforeBan)
    {
        foreach (var order in ordersCancelBeforeBan)
        {
            order.Status = OrderStatus.Cancelled;
            order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_SHOP_CANCEL.GetDescription();
            var payment = order.Payments.FirstOrDefault(p => p.PaymentMethods == PaymentMethods.VnPay && p.Type == PaymentTypes.Payment && p.Status == PaymentStatus.PaidSuccess);
            if (payment != default)
            {
                order.IsRefund = await RefundOrderAsync(order, payment).ConfigureAwait(false);
            }

            _orderRepository.Update(order);
        }
    }

    private async Task CancelOrderPendingOrConfirmedForBanShop(List<Order> ordersCancelBeforeBan)
    {
        foreach (var order in ordersCancelBeforeBan)
        {
            order.Status = OrderStatus.Cancelled;
            order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_CUSTOMER_CANCEL.GetDescription();
            var payment = order.Payments.FirstOrDefault(p => p.PaymentMethods == PaymentMethods.VnPay && p.Type == PaymentTypes.Payment && p.Status == PaymentStatus.PaidSuccess);
            if (payment != default)
            {
                order.IsRefund = await RefundOrderAsync(order, payment).ConfigureAwait(false);
            }

            _orderRepository.Update(order);
        }
    }

    private async Task<bool> RefundOrderAsync(Order order, Payment payment)
    {
        if (payment.PaymentMethods == PaymentMethods.VnPay && payment.Status == PaymentStatus.PaidSuccess)
        {
            // Refund + update status order to cancel
            var refundPayment = new Payment
            {
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Status = PaymentStatus.Pending,
                Type = PaymentTypes.Refund,
                PaymentMethods = PaymentMethods.VnPay,
            };
            var refundResult = await _vnPayPaymentService.CreateRefund(payment).ConfigureAwait(false);
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
            var content = JsonSerializer.Serialize(refundResult, options);

            refundPayment.PaymentThirdPartyId = refundResult.VnpTransactionNo;
            refundPayment.PaymentThirdPartyContent = content;

            if (refundResult.VnpResponseCode == ((int)VnPayRefundResponseCode.CODE_00).ToString("D2"))
            {
                refundPayment.Status = PaymentStatus.PaidSuccess;

                // Rút tiền từ ví hoa hồng về ví hệ thống sau đó refund tiền về cho customer
                var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
                var systemCommissionWallet = await _walletRepository.GetByType(WalletTypes.SystemCommission).ConfigureAwait(false);
                var listWalletTransaction = new List<WalletTransaction>();

                WalletTransaction transactionWithdrawalSystemCommissionToSystemTotal = new WalletTransaction
                {
                    WalletFromId = systemCommissionWallet.Id,
                    WalletToId = systemTotalWallet.Id,
                    AvaiableAmountBefore = systemCommissionWallet.AvailableAmount,
                    IncomingAmountBefore = systemCommissionWallet.IncomingAmount,
                    ReportingAmountBefore = systemCommissionWallet.ReportingAmount,
                    Amount = -order.ChargeFee,
                    Type = WalletTransactionType.Withdrawal,
                    Description = $"Rút tiền từ ví hoa hồng {MoneyUtils.FormatMoneyWithDots(order.ChargeFee)} VNĐ về ví tổng hệ thống",
                };

                systemCommissionWallet.AvailableAmount -= order.ChargeFee;
                listWalletTransaction.Add(transactionWithdrawalSystemCommissionToSystemTotal);

                WalletTransaction transactionAddFromSystemCommissionToSystemTotal = new WalletTransaction
                {
                    WalletFromId = systemCommissionWallet.Id,
                    WalletToId = systemTotalWallet.Id,
                    AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
                    IncomingAmountBefore = systemTotalWallet.IncomingAmount,
                    ReportingAmountBefore = systemTotalWallet.ReportingAmount,
                    Amount = order.ChargeFee,
                    Type = WalletTransactionType.Transfer,
                    Description = $"Tiền từ ví hoa hồng chuyển về ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(order.ChargeFee)} VNĐ",
                };
                systemTotalWallet.AvailableAmount += order.ChargeFee;
                listWalletTransaction.Add(transactionAddFromSystemCommissionToSystemTotal);

                WalletTransaction transactionWithdrawalSystemTotalForRefundPaymentOnline = new WalletTransaction
                {
                    WalletFromId = systemTotalWallet.Id,
                    AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
                    IncomingAmountBefore = systemTotalWallet.IncomingAmount,
                    ReportingAmountBefore = systemTotalWallet.ReportingAmount,
                    Amount = -payment.Amount,
                    Type = WalletTransactionType.Withdrawal,
                    Description = $"Rút tiền từ ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(payment.Amount)} VNĐ để hoàn tiền giao dịch thanh toán online của đơn hàng MS-{payment.OrderId}",
                };
                systemTotalWallet.AvailableAmount -= payment.Amount;
                listWalletTransaction.Add(transactionWithdrawalSystemTotalForRefundPaymentOnline);

                refundPayment.WalletTransactions = listWalletTransaction;
                _walletRepository.Update(systemTotalWallet);
                _walletRepository.Update(systemCommissionWallet);
            }
            else
            {
                refundPayment.Status = PaymentStatus.PaidFail;

                // Get moderator account to send mail
                await SendEmailAnnounceModeratorAsync(order).ConfigureAwait(false);

                // Send notification for moderator
                NotifyAnnounceRefundFailAsync(order);
            }

            await _paymentRepository.AddAsync(refundPayment).ConfigureAwait(false);
            return true;
        }

        return false;
    }

    private async Task SendEmailAnnounceModeratorAsync(Order order)
    {
        var building = _buildingRepository.GetById(order.BuildingId);
        var moderators = _accountRepository.GetAccountsOfModeratorByDormitoryId(building!.DormitoryId);
        if (moderators != default && moderators.Count > 0)
        {
            foreach (var moderator in moderators)
            {
                _emailService.SendEmailToAnnounceModeratorRefundFail(moderator.Email, order.Id);
            }
        }
    }

    private void NotifyAnnounceRefundFailAsync(Order order)
    {
        var building = _buildingRepository.GetById(order.BuildingId);
        var moderators = _accountRepository.GetAccountsOfModeratorByDormitoryId(building!.DormitoryId);
        if (moderators != default && moderators.Count > 0)
        {
            foreach (var moderator in moderators)
            {
                var notification = _notificationFactory.CreateRefundFaillNotification(order, moderator);
                _notifierService.NotifyAsync(notification);
            }
        }
    }
}