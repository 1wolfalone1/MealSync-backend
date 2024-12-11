using System.Text;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Chat;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Services.Payments.VnPay;
using MealSync.Application.Common.Services.Payments.VnPay.Models;
using MealSync.Application.Common.Services.Payments.VnPay.Shared;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.UpdatePaymentStatusIPN;

public class UpdatePaymentStatusIPNHandler : ICommandHandler<UpdatePaymentStatusIPNCommand, VnPayIPNResponse>
{
    private readonly IVnPayPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IShopRepository _shopRepository;
    private readonly ILogger<UpdatePaymentStatusIPNHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IDepositRepository _depositRepository;

    public UpdatePaymentStatusIPNHandler(
        IVnPayPaymentService paymentService, IPaymentRepository paymentRepository,
        ILogger<UpdatePaymentStatusIPNHandler> logger, IUnitOfWork unitOfWork,
        IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository,
        IShopRepository shopRepository, INotificationFactory notificationFactory,
        INotifierService notifierService, ISystemResourceRepository systemResourceRepository, IDepositRepository depositRepository, IOrderRepository orderRepository, IChatService chatService, IAccountRepository accountRepository)
    {
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _shopRepository = shopRepository;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _systemResourceRepository = systemResourceRepository;
        _depositRepository = depositRepository;
    }

    public async Task<Result<VnPayIPNResponse>> Handle(UpdatePaymentStatusIPNCommand request, CancellationToken cancellationToken)
    {
        var parsePaymentId = Int64.TryParse(request.Query[VnPayRequestParam.VNP_TXN_REF], out var paymentId);
        if (parsePaymentId)
        {
            var paymentType = await _paymentService.GetPaymentType(request.Query).ConfigureAwait(false);
            if (paymentType.Contains(VnPayPaymentType.ORDER_PAYMENT))
            {
                var payment = await _paymentRepository.GetOrderPaymentVnPayById(paymentId).ConfigureAwait(false);
                var response = await _paymentService.GetIPNPaymentOrder(request.Query, payment).ConfigureAwait(false);
                var shop = await _shopRepository.GetByAccountId(payment.Order.ShopId).ConfigureAwait(false);

                if (response.RspCode == ((int)VnPayIPNResponseCode.CODE_00).ToString("D2"))
                {
                    try
                    {
                        // Begin transaction
                        await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                        payment.PaymentThirdPartyId = request.Query[VnPayRequestParam.VNP_TRANSACTION_NO];
                        payment.PaymentThirdPartyContent = ConvertQueryCollectionToString(request.Query);

                        if (response.Message == "Confirm Success")
                        {
                            payment.Status = PaymentStatus.PaidSuccess;
                            payment.Order.Status = OrderStatus.Pending;

                            if (shop.IsAutoOrderConfirmation && shop.Status == ShopStatus.Active)
                            {
                                var intendedReceiveDateTime = new DateTime(
                                    payment.Order.IntendedReceiveDate.Year,
                                    payment.Order.IntendedReceiveDate.Month,
                                    payment.Order.IntendedReceiveDate.Day,
                                    payment.Order.StartTime / 100,
                                    payment.Order.StartTime % 100,
                                    0);

                                var minAllowed = new DateTimeOffset(intendedReceiveDateTime, TimeSpan.FromHours(7)).AddHours(-shop.MaxOrderHoursInAdvance);
                                var maxAllowed = new DateTimeOffset(intendedReceiveDateTime, TimeSpan.FromHours(7)).AddHours(-shop.MinOrderHoursInAdvance);

                                if (payment.Order.CreatedDate.ToOffset(TimeSpan.FromHours(7)) >= minAllowed && payment.Order.CreatedDate.ToOffset(TimeSpan.FromHours(7)) <= maxAllowed)
                                {
                                    payment.Order.Status = OrderStatus.Confirmed;
                                }
                            }

                            var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
                            var systemCommissionWallet = await _walletRepository.GetByType(WalletTypes.SystemCommission).ConfigureAwait(false);
                            var walletTransactions = new List<WalletTransaction>();

                            WalletTransaction transactionToSystemTotal = new WalletTransaction
                            {
                                WalletToId = systemTotalWallet.Id,
                                PaymentId = payment.Id,
                                AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
                                IncomingAmountBefore = systemTotalWallet.IncomingAmount,
                                ReportingAmountBefore = systemTotalWallet.ReportingAmount,
                                Amount = payment.Amount,
                                Type = WalletTransactionType.Transfer,
                                Description = $"Tiền VNPAY về ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(payment.Amount)} VNĐ",
                            };

                            systemTotalWallet.AvailableAmount += payment.Amount;
                            walletTransactions.Add(transactionToSystemTotal);

                            WalletTransaction transactionWithdrawalSystemTotalToSystemCommission = new WalletTransaction
                            {
                                WalletFromId = systemTotalWallet.Id,
                                WalletToId = systemCommissionWallet.Id,
                                AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
                                IncomingAmountBefore = systemTotalWallet.IncomingAmount,
                                ReportingAmountBefore = systemTotalWallet.ReportingAmount,
                                Amount = -payment.Order.ChargeFee,
                                Type = WalletTransactionType.Withdrawal,
                                Description = $"Rút tiền từ ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(payment.Order.ChargeFee)} VNĐ về ví hoa hồng",
                            };

                            systemTotalWallet.AvailableAmount -= payment.Order.ChargeFee;
                            walletTransactions.Add(transactionWithdrawalSystemTotalToSystemCommission);

                            WalletTransaction transactionAddFromSystemTotalToSystemCommission = new WalletTransaction
                            {
                                WalletFromId = systemTotalWallet.Id,
                                WalletToId = systemCommissionWallet.Id,
                                AvaiableAmountBefore = systemCommissionWallet.AvailableAmount,
                                IncomingAmountBefore = systemCommissionWallet.IncomingAmount,
                                ReportingAmountBefore = systemCommissionWallet.ReportingAmount,
                                Amount = payment.Order.ChargeFee,
                                Type = WalletTransactionType.Transfer,
                                Description = $"Tiền từ ví tổng hệ thống chuyển về ví hoa hồng {MoneyUtils.FormatMoneyWithDots(payment.Order.ChargeFee)} VNĐ",
                            };

                            systemCommissionWallet.AvailableAmount += payment.Order.ChargeFee;
                            walletTransactions.Add(transactionAddFromSystemTotalToSystemCommission);

                            await _walletTransactionRepository.AddRangeAsync(walletTransactions).ConfigureAwait(false);
                            _walletRepository.Update(systemTotalWallet);
                            _walletRepository.Update(systemCommissionWallet);

                            // Notify
                            if (payment.Order.Status == OrderStatus.Pending)
                            {
                                var notification = _notificationFactory.CreateOrderPendingNotification(payment.Order, shop);
                                _notifierService.NotifyAsync(notification);
                            }
                            else if (payment.Order.Status == OrderStatus.Confirmed)
                            {
                                var notification = _notificationFactory.CreateOrderConfirmedNotification(payment.Order, shop);
                                _notifierService.NotifyAsync(notification);
                            }
                            else
                            {
                                // Do nothing
                            }
                        }
                        else
                        {
                            payment.Status = PaymentStatus.PaidFail;
                            payment.Order.Status = OrderStatus.PendingPayment;
                            payment.Order.Reason = _systemResourceRepository.GetByResourceCode(MessageCode.I_PAYMENT_FAIL.GetDescription());
                            response.Message = "Confirm Success";
                        }

                        _paymentRepository.Update(payment);

                        await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        // Rollback when exception
                        _unitOfWork.RollbackTransaction();
                        _logger.LogError(e, e.Message);
                        throw new("Internal Server Error");
                    }
                }

                return Result.Success(response);
            }
            else if (paymentType.Contains(VnPayPaymentType.DEPOSIT))
            {
                var deposit = await _depositRepository.GetByIdAsync(paymentId).ConfigureAwait(false)!;
                var response = await _paymentService.GetIPNDeposit(request.Query, deposit).ConfigureAwait(false);

                if (response.RspCode == ((int)VnPayIPNResponseCode.CODE_00).ToString("D2"))
                {
                    try
                    {
                        // Begin transaction
                        await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                        deposit.PaymentThirdPartyId = request.Query[VnPayRequestParam.VNP_TRANSACTION_NO];
                        deposit.PaymentThirdPartyContent = ConvertQueryCollectionToString(request.Query);

                        if (response.Message == "Confirm Success")
                        {
                            deposit.Status = DepositStatus.Success;

                            var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
                            var shopWallet = _walletRepository.GetById(deposit.WalletId)!;
                            var walletTransactions = new List<WalletTransaction>();

                            WalletTransaction transactionToSystemTotal = new WalletTransaction
                            {
                                WalletToId = systemTotalWallet.Id,
                                DepositId = deposit.Id,
                                AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
                                IncomingAmountBefore = systemTotalWallet.IncomingAmount,
                                ReportingAmountBefore = systemTotalWallet.ReportingAmount,
                                Amount = deposit.Amount,
                                Type = WalletTransactionType.Transfer,
                                Description = $"Tiền VNPAY về ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(deposit.Amount)} VNĐ",
                            };

                            systemTotalWallet.AvailableAmount += deposit.Amount;
                            walletTransactions.Add(transactionToSystemTotal);

                            WalletTransaction transactionWithdrawalSystemTotalToShopWallet = new WalletTransaction
                            {
                                WalletFromId = systemTotalWallet.Id,
                                WalletToId = shopWallet.Id,
                                DepositId = deposit.Id,
                                AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
                                IncomingAmountBefore = systemTotalWallet.IncomingAmount,
                                ReportingAmountBefore = systemTotalWallet.ReportingAmount,
                                Amount = -deposit.Amount,
                                Type = WalletTransactionType.Withdrawal,
                                Description = $"Rút tiền từ ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(deposit.Amount)} VNĐ về ví tiền có sẵn từ yêu cầu nạp tiền MS-{deposit.Id}",
                            };

                            systemTotalWallet.AvailableAmount -= deposit.Amount;
                            walletTransactions.Add(transactionWithdrawalSystemTotalToShopWallet);

                            WalletTransaction transactionAddFromSystemTotalToShopWallet = new WalletTransaction
                            {
                                WalletFromId = systemTotalWallet.Id,
                                WalletToId = shopWallet.Id,
                                DepositId = deposit.Id,
                                AvaiableAmountBefore = shopWallet.AvailableAmount,
                                IncomingAmountBefore = shopWallet.IncomingAmount,
                                ReportingAmountBefore = shopWallet.ReportingAmount,
                                Amount = deposit.Amount,
                                Type = WalletTransactionType.Transfer,
                                Description = $"Tiền từ ví tổng hệ thống chuyển về ví tiền có sẵn {MoneyUtils.FormatMoneyWithDots(deposit.Amount)} VNĐ",
                            };

                            shopWallet.AvailableAmount += deposit.Amount;
                            walletTransactions.Add(transactionAddFromSystemTotalToShopWallet);

                            await _walletTransactionRepository.AddRangeAsync(walletTransactions).ConfigureAwait(false);
                            _walletRepository.Update(systemTotalWallet);
                            _walletRepository.Update(shopWallet);

                        }
                        else
                        {
                            deposit.Status = DepositStatus.Failed;
                            response.Message = "Confirm Success";
                        }

                        _depositRepository.Update(deposit);

                        await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        // Rollback when exception
                        _unitOfWork.RollbackTransaction();
                        _logger.LogError(e, e.Message);
                        throw new("Internal Server Error");
                    }
                }

                return Result.Success(response);
            }
            else
            {
                var response = new VnPayIPNResponse();
                response.RspCode = ((int)VnPayIPNResponseCode.CODE_02).ToString("D2");
                response.Message = VnPayIPNResponseCode.CODE_02.GetDescription();
                return response;
            }
        }
        else
        {
            var response = new VnPayIPNResponse();
            response.RspCode = ((int)VnPayIPNResponseCode.CODE_02).ToString("D2");
            response.Message = VnPayIPNResponseCode.CODE_02.GetDescription();
            return response;
        }
    }

    private string ConvertQueryCollectionToString(IQueryCollection query)
    {
        var stringBuilder = new StringBuilder();

        foreach (var key in query.Keys)
        {
            var values = query[key]; // IQueryCollection may contain multiple values for a key.
            foreach (var value in values)
            {
                stringBuilder.Append($"{key}={value}&");
            }
        }

        // Remove the trailing '&' if it exists.
        if (stringBuilder.Length > 0)
        {
            stringBuilder.Length--;
        }

        return stringBuilder.ToString();
    }
}