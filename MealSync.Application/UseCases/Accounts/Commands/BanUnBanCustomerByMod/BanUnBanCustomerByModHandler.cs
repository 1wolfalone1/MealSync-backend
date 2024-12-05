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

namespace MealSync.Application.UseCases.Accounts.Commands.BanUnBanCustomerByMod;

public class BanUnBanCustomerByModHandler : ICommandHandler<BanUnBanCustomerByModCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IVnPayPaymentService _vnPayPaymentService;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly ILogger<BanUnBanCustomerByModHandler> _logger;

    public BanUnBanCustomerByModHandler(
        ICustomerRepository customerRepository, IModeratorDormitoryRepository moderatorDormitoryRepository,
        IOrderRepository orderRepository, ISystemResourceRepository systemResourceRepository,
        IAccountRepository accountRepository, IBuildingRepository buildingRepository,
        IWalletTransactionRepository walletTransactionRepository, IWalletRepository walletRepository,
        IPaymentRepository paymentRepository, IVnPayPaymentService vnPayPaymentService,
        INotificationFactory notificationFactory, INotifierService notifierService, ICurrentPrincipalService currentPrincipalService,
        IEmailService emailService, IUnitOfWork unitOfWork, ILogger<BanUnBanCustomerByModHandler> logger, ISystemConfigRepository systemConfigRepository)
    {
        _customerRepository = customerRepository;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _orderRepository = orderRepository;
        _systemResourceRepository = systemResourceRepository;
        _accountRepository = accountRepository;
        _buildingRepository = buildingRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _walletRepository = walletRepository;
        _paymentRepository = paymentRepository;
        _vnPayPaymentService = vnPayPaymentService;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _currentPrincipalService = currentPrincipalService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _systemConfigRepository = systemConfigRepository;
    }

    public async Task<Result<Result>> Handle(BanUnBanCustomerByModCommand request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();

        var customer = await _customerRepository.GetCustomer(dormitoryIds, request.Id).ConfigureAwait(false);
        if (customer == default)
        {
            throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
        }
        else
        {
            if (request.Status == AccountStatus.Banned && customer.Account.Status == AccountStatus.Verify)
            {
                // Ban
                var totalOrderInProcess = await _orderRepository.CountTotalOrderInProcessByCustomerId(customer.Id).ConfigureAwait(false);
                var ordersCancelBeforeBan = await _orderRepository.GetForSystemCancelByCustomerId(customer.Id).ConfigureAwait(false);

                if (totalOrderInProcess > 0 && customer.Status == CustomerStatus.Banning && request.Status == AccountStatus.Banned)
                {
                    throw new InvalidBusinessException(MessageCode.E_MODERATOR_CAN_NOT_UPDATE_STATUS_CUSTOMER_TO_BANNED.GetDescription());
                }
                else if (!request.IsConfirm && totalOrderInProcess > 0)
                {
                    return Result.Warning(new
                    {
                        Code = MessageCode.W_MODERATOR_UPDATE_STATUS_CUSTOMER_TO_BANNING.GetDescription(),
                        Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_MODERATOR_UPDATE_STATUS_CUSTOMER_TO_BANNING.GetDescription()),
                    });
                }
                else
                {
                    try
                    {
                        // Begin transaction
                        await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                        if (totalOrderInProcess > 0)
                        {
                            customer.Status = CustomerStatus.Banning;
                            _customerRepository.Update(customer);
                            await CancelOrderPendingOrConfirmed(ordersCancelBeforeBan).ConfigureAwait(false);
                        }
                        else
                        {
                            customer.Status = CustomerStatus.Banned;
                            customer.Account.Status = AccountStatus.Banned;
                            _customerRepository.Update(customer);
                            await CancelOrderPendingOrConfirmed(ordersCancelBeforeBan).ConfigureAwait(false);
                        }

                        // Commit transaction
                        await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                        // Send mail ban for customer
                        _emailService.SendBanCustomerWithReason(customer.Account.Email, customer.Account.FullName, request.Reason, customer.Account.NumOfFlag, customer.Status == CustomerStatus.Banned);

                        // Notify cancel order for shop
                        foreach (var order in ordersCancelBeforeBan)
                        {
                            var notification = _notificationFactory.CreateCustomerCancelOrderNotification(order, customer.Account);
                            _notifierService.NotifyAsync(notification);
                        }
                    }
                    catch (Exception e)
                    {
                        // Rollback when exception
                        _unitOfWork.RollbackTransaction();
                        _logger.LogError(e, e.Message);
                        throw new("Internal Server Error");
                    }
                }

                return Result.Success(new
                {
                    Code = MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription()),
                });
            }
            else if (request.Status == AccountStatus.Verify && (customer.Status == CustomerStatus.Banning || customer.Status == CustomerStatus.Banned))
            {
                var systemConfig = _systemConfigRepository.GetSystemConfig();

                // UnBan
                try
                {
                    // Begin transaction
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                    customer.Account.Status = AccountStatus.Verify;
                    customer.Status = CustomerStatus.Active;

                    if (customer.Account.NumOfFlag >= systemConfig.MaxFlagsBeforeBan)
                    {
                        customer.Account.NumOfFlag -= 1;
                    }

                    _customerRepository.Update(customer);

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                    // Send Email UnBan
                    _emailService.SendUnBanCustomerWithReason(customer.Account.Email, customer.Account.FullName, request.Reason);
                }
                catch (Exception e)
                {
                    // Rollback when exception
                    _unitOfWork.RollbackTransaction();
                    _logger.LogError(e, e.Message);
                    throw new("Internal Server Error");
                }

                return Result.Success(new
                {
                    Code = MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription()),
                });
            }
            else
            {
                throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
            }
        }
    }

    private async Task CancelOrderPendingOrConfirmed(List<Order> ordersCancelBeforeBan)
    {

        foreach (var order in ordersCancelBeforeBan)
        {
            order.Status = OrderStatus.Cancelled;
            order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_SHOP_CANCEL.GetDescription();

            var payment = order.Payments.FirstOrDefault(p => p.PaymentMethods == PaymentMethods.VnPay && p.Type == PaymentTypes.Payment && p.Status == PaymentStatus.PaidSuccess);
            if (payment != default)
            {
                var refundPayment = new Payment
                {
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    Status = PaymentStatus.Pending,
                    Type = PaymentTypes.Refund,
                    PaymentMethods = PaymentMethods.VnPay,
                };
                var refundResult = await _vnPayPaymentService.CreateRefund(payment).ConfigureAwait(false);
                var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
                var content = JsonSerializer.Serialize(refundResult, options);

                refundPayment.PaymentThirdPartyId = refundResult.VnpTransactionNo;
                refundPayment.PaymentThirdPartyContent = content;
                order.IsRefund = true;

                if (refundResult.VnpResponseCode == ((int)VnPayRefundResponseCode.CODE_00).ToString("D2"))
                {
                    refundPayment.Status = PaymentStatus.PaidSuccess;

                    // Rút tiền từ ví hoa hồng về ví hệ thống sau đó refund tiền về cho customer
                    var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
                    var systemCommissionWallet = await _walletRepository.GetByType(WalletTypes.SystemCommission).ConfigureAwait(false);

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

                    await _walletTransactionRepository.AddAsync(transactionWithdrawalSystemCommissionToSystemTotal).ConfigureAwait(false);
                    await _walletTransactionRepository.AddAsync(transactionAddFromSystemCommissionToSystemTotal).ConfigureAwait(false);
                    await _walletTransactionRepository.AddAsync(transactionWithdrawalSystemTotalForRefundPaymentOnline).ConfigureAwait(false);
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
            }

            _orderRepository.Update(order);
        }
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