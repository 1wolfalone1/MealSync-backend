using System.Text;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services.Payments.VnPay;
using MealSync.Application.Common.Services.Payments.VnPay.Models;
using MealSync.Application.Common.Services.Payments.VnPay.Shared;
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
    private readonly ICommissionConfigRepository _commissionConfigRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly ILogger<UpdatePaymentStatusIPNHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePaymentStatusIPNHandler(
        IVnPayPaymentService paymentService, IPaymentRepository paymentRepository,
        ILogger<UpdatePaymentStatusIPNHandler> logger, IUnitOfWork unitOfWork,
        IWalletRepository walletRepository, ICommissionConfigRepository commissionConfigRepository,
        IWalletTransactionRepository walletTransactionRepository)
    {
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _walletRepository = walletRepository;
        _commissionConfigRepository = commissionConfigRepository;
        _walletTransactionRepository = walletTransactionRepository;
    }

    public async Task<Result<VnPayIPNResponse>> Handle(UpdatePaymentStatusIPNCommand request, CancellationToken cancellationToken)
    {
        var parseOrderId = Int64.TryParse(request.Query[VnPayRequestParam.VNP_TXN_REF], out var orderId);
        if (parseOrderId)
        {
            var payment = await _paymentRepository.GetOrderPaymentVnPayByOrderId(orderId).ConfigureAwait(false);
            var response = await _paymentService.GetIPN(request.Query, payment).ConfigureAwait(false);
            var commissionConfig = _commissionConfigRepository.GetCommissionConfig();
            if (response.RspCode == ((int)VnPayIPNResponseCode.CODE_00).ToString("D2"))
            {
                if (response.Message == "Confirm Success")
                {
                    payment.Status = PaymentStatus.PaidSuccess;
                    payment.PaymentThirdPartyId = request.Query[VnPayRequestParam.VNP_TRANSACTION_NO];
                    payment.PaymentThirdPartyContent = ConvertQueryCollectionToString(request.Query);
                }
                else
                {
                    payment.Status = PaymentStatus.PaidFail;
                    response.Message = "Confirm Success";
                }

                try
                {
                    // Begin transaction
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                    _paymentRepository.Update(payment);

                    var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
                    var systemCommissionWallet = await _walletRepository.GetByType(WalletTypes.SystemCommission).ConfigureAwait(false);
                    var shopWallet = _walletRepository.GetById(payment.Order.Shop.WalletId);
                    WalletTransaction transactionToSystemTotal = new WalletTransaction
                    {
                        WalletToId = systemTotalWallet.Id,
                        PaymentId = payment.Id,
                        AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
                        IncomingAmountBefore = systemTotalWallet.IncomingAmount,
                        ReportingAmountBefore = systemTotalWallet.ReportingAmount,
                        Amount = payment.Amount,
                        Type = WalletTransactionType.Transfer,
                        Description = $"Tiền VNPAY về ví tổng hệ thống {payment.Amount} VNĐ",
                    };

                    systemTotalWallet.AvailableAmount += payment.Amount;

                    WalletTransaction transactionWithdrawalSystemTotalToSystemCommission = new WalletTransaction
                    {
                        WalletFromId = systemTotalWallet.Id,
                        WalletToId = systemCommissionWallet.Id,
                        AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
                        IncomingAmountBefore = systemTotalWallet.IncomingAmount,
                        ReportingAmountBefore = systemTotalWallet.ReportingAmount,
                        Amount = payment.Order.ChargeFee,
                        Type = WalletTransactionType.Withdrawal,
                        Description = $"Rút tiền từ ví tổng hệ thống {payment.Order.ChargeFee} VNĐ về ví hoa hồng",
                    };

                    systemTotalWallet.AvailableAmount -= payment.Order.ChargeFee;

                    WalletTransaction transactionAddFromSystemTotalToSystemCommission = new WalletTransaction
                    {
                        WalletFromId = systemTotalWallet.Id,
                        WalletToId = systemCommissionWallet.Id,
                        AvaiableAmountBefore = systemCommissionWallet.AvailableAmount,
                        IncomingAmountBefore = systemCommissionWallet.IncomingAmount,
                        ReportingAmountBefore = systemCommissionWallet.ReportingAmount,
                        Amount = payment.Order.ChargeFee,
                        Type = WalletTransactionType.Transfer,
                        Description = $"Tiền từ ví tổng hệ thống chuyển về ví hoa hồng {payment.Order.ChargeFee} VNĐ",
                    };

                    systemCommissionWallet.AvailableAmount += payment.Order.ChargeFee;

                    var orderPrice = payment.Order.TotalPrice - payment.Order.TotalPromotion - payment.Order.ChargeFee;
                    WalletTransaction transactionWithdrawalSystemTotalToShop = new WalletTransaction
                    {
                        WalletFromId = systemTotalWallet.Id,
                        WalletToId = shopWallet.Id,
                        AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
                        IncomingAmountBefore = systemTotalWallet.IncomingAmount,
                        ReportingAmountBefore = systemTotalWallet.ReportingAmount,
                        Amount = orderPrice,
                        Type = WalletTransactionType.Withdrawal,
                        Description = $"Rút tiền từ ví tổng hệ thống {orderPrice} VNĐ về ví shop",
                    };

                    systemTotalWallet.AvailableAmount -= orderPrice;

                    WalletTransaction transactionAddFromSystemTotalToShop = new WalletTransaction
                    {
                        WalletFromId = systemTotalWallet.Id,
                        WalletToId = shopWallet.Id,
                        AvaiableAmountBefore = shopWallet.AvailableAmount,
                        IncomingAmountBefore = shopWallet.IncomingAmount,
                        ReportingAmountBefore = shopWallet.ReportingAmount,
                        Amount = orderPrice,
                        Type = WalletTransactionType.Transfer,
                        Description = $"Tiền từ ví tổng hệ thống chuyển về ví shop {orderPrice} VNĐ",
                    };

                    shopWallet.AvailableAmount += orderPrice;
                    await _walletTransactionRepository.AddAsync(transactionToSystemTotal).ConfigureAwait(false);
                    await _walletTransactionRepository.AddAsync(transactionWithdrawalSystemTotalToSystemCommission).ConfigureAwait(false);
                    await _walletTransactionRepository.AddAsync(transactionAddFromSystemTotalToSystemCommission).ConfigureAwait(false);
                    await _walletTransactionRepository.AddAsync(transactionWithdrawalSystemTotalToShop).ConfigureAwait(false);
                    await _walletTransactionRepository.AddAsync(transactionAddFromSystemTotalToShop).ConfigureAwait(false);
                    _walletRepository.Update(systemTotalWallet);
                    _walletRepository.Update(systemCommissionWallet);
                    _walletRepository.Update(shopWallet);

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