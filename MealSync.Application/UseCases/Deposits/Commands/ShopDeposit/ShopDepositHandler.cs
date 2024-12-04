using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Payments.VnPay;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Deposits.Commands.ShopDeposit;

public class ShopDepositHandler : ICommandHandler<ShopDepositCommand, Result>
{
    private readonly IDepositRepository _depositRepository;
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IVnPayPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShopDepositHandler> _logger;

    public ShopDepositHandler(
        IDepositRepository depositRepository, IShopRepository shopRepository,
        ICurrentPrincipalService currentPrincipalService, IVnPayPaymentService paymentService,
        IUnitOfWork unitOfWork, ILogger<ShopDepositHandler> logger)
    {
        _depositRepository = depositRepository;
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(ShopDepositCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shop = await _shopRepository.GetByAccountId(shopId).ConfigureAwait(false);
        try
        {
            Deposit deposit = new Deposit
            {
                WalletId = shop.WalletId,
                Amount = request.Amount,
                Status = DepositStatus.Pending,
                Description = $"Yêu cầu nạp tiền {MoneyUtils.FormatMoneyWithDots(request.Amount)} VNĐ vào ví có sẵn",
            };

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            await _depositRepository.AddAsync(deposit).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            var paymentUrl = await _paymentService.CreatePaymentDepositUrl(deposit).ConfigureAwait(false);
            return Result.Success(new
            {
                PaymentUrl = paymentUrl,
            });
        }
        catch (Exception e)
        {
            // Rollback when exception
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }
    }
}