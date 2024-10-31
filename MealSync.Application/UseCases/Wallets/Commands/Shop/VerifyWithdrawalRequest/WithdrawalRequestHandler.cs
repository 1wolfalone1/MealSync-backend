using System.Text.Json;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Wallets.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Wallets.Commands.Shop.VerifyWithdrawalRequest;

public class WithdrawalRequestHandler : ICommandHandler<WithdrawalRequestCommand, Result>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IShopDormitoryRepository _shopDormitoryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<WithdrawalRequestHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public WithdrawalRequestHandler(
        IWalletRepository walletRepository, IShopRepository shopRepository,
        IWithdrawalRequestRepository withdrawalRequestRepository, ISystemResourceRepository systemResourceRepository,
        IShopDormitoryRepository shopDormitoryRepository, IAccountRepository accountRepository,
        ICurrentPrincipalService currentPrincipalService, INotificationFactory notificationFactory,
        INotifierService notifierService, IMapper mapper, ILogger<WithdrawalRequestHandler> logger,
        IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _walletRepository = walletRepository;
        _shopRepository = shopRepository;
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _systemResourceRepository = systemResourceRepository;
        _shopDormitoryRepository = shopDormitoryRepository;
        _accountRepository = accountRepository;
        _currentPrincipalService = currentPrincipalService;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Result<Result>> Handle(WithdrawalRequestCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shop = await _shopRepository.GetByAccountId(shopId).ConfigureAwait(false);
        var account = _accountRepository.GetById(shopId)!;
        var wallet = _walletRepository.GetById(shop.WalletId);

        if (wallet.AvailableAmount <= 0)
        {
            throw new InvalidBusinessException(MessageCode.E_WITHDRAWAL_NOT_ENOUGH_AVAILABLE_AMOUNT.GetDescription());
        }
        else if (request.Amount > wallet.AvailableAmount)
        {
            throw new InvalidBusinessException(MessageCode.E_WITHDRAWAL_AMOUNT_MUST_LESS_THAN_OR_EQUAL_AVAILABLE_AMOUNT.GetDescription(), new object[] { MoneyUtils.FormatMoneyWithDots(wallet.AvailableAmount) });
        }
        else
        {
            var cacheKey = GenerateCacheKey(VerificationCodeTypes.Withdrawal, account.Email);
            var cachedValue = await _cacheService.GetCachedResponseAsync(cacheKey).ConfigureAwait(false);
            if (cachedValue == default || cachedValue.Length == 0)
            {
                throw new InvalidBusinessException(MessageCode.E_WITHDRAWAL_INVALID_CODE.GetDescription());
            }

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var withdrawalRequestCacheDto = JsonSerializer.Deserialize<WithdrawalRequestCacheDto>(cachedValue, options);

            if (withdrawalRequestCacheDto == default)
            {
                throw new InvalidBusinessException(MessageCode.E_WITHDRAWAL_INVALID_CODE.GetDescription());
            }
            else if (withdrawalRequestCacheDto.Code != request.VerifyCode.ToString())
            {
                throw new InvalidBusinessException(MessageCode.E_WITHDRAWAL_INVALID_CODE.GetDescription());
            }
            else if (withdrawalRequestCacheDto.Data.Amount != request.Amount
                     || withdrawalRequestCacheDto.Data.BankCode != request.BankCode
                     || withdrawalRequestCacheDto.Data.BankAccountNumber != request.BankAccountNumber
                     || withdrawalRequestCacheDto.Data.BankShortName != request.BankShortName)
            {
                throw new InvalidBusinessException(MessageCode.E_WITHDRAWAL_INVALID_CODE.GetDescription());
            }
            else
            {
                var result = await InsertWithdrawalRequestData(request, shop, wallet).ConfigureAwait(false);
                await _cacheService.RemoveCacheResponseAsync(cacheKey).ConfigureAwait(false);
                return result;
            }
        }
    }

    private async Task<Result<Result>> InsertWithdrawalRequestData(WithdrawalRequestCommand request, Domain.Entities.Shop shop, Wallet wallet)
    {
        var description = $"Yêu cầu rút tiền từ shop id {shop.Id} với số tiền {MoneyUtils.FormatMoneyWithDots(request.Amount)} VNĐ";
        var walletTransaction = new WalletTransaction
        {
            AvaiableAmountBefore = wallet.AvailableAmount,
            IncomingAmountBefore = wallet.IncomingAmount,
            ReportingAmountBefore = wallet.ReportingAmount,
            Amount = request.Amount,
            Type = WalletTransactionType.Withdrawal,
            Description = description,
        };
        var withdrawalRequest = new WithdrawalRequest
        {
            WalletId = wallet.Id,
            Amount = request.Amount,
            Status = WithdrawalRequestStatus.Pending,
            BankShortName = request.BankShortName,
            BankCode = request.BankCode,
            BankAccountNumber = request.BankAccountNumber,
            WalletTransaction = walletTransaction,
        };

        wallet.AvailableAmount -= request.Amount;

        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

            _walletRepository.Update(wallet);
            await _withdrawalRequestRepository.AddAsync(withdrawalRequest).ConfigureAwait(false);

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // Rollback when exception
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }

        var shopDormitories = await _shopDormitoryRepository.GetByShopId(shop.Id).ConfigureAwait(false);
        foreach (var shopDormitory in shopDormitories)
        {
            var moderators = _accountRepository.GetAccountsOfModeratorByDormitoryId(shopDormitory.DormitoryId);
            foreach (var moderator in moderators)
            {
                var notifyToModerator = _notificationFactory.CreateWithdrawalRequestToModeratorNotification(withdrawalRequest, moderator, shop, description);
                await _notifierService.NotifyAsync(notifyToModerator).ConfigureAwait(false);
            }
        }

        return Result.Success(new
        {
            Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_WITHDRAWAL_REQUEST_SUCCESS.GetDescription()),
            WalletInfo = _mapper.Map<WalletSummaryResponse>(wallet),
        });
    }

    private string GenerateCacheKey(VerificationCodeTypes verificationCodeType, string email)
    {
        return verificationCodeType.GetDescription() + "-" + email;
    }
}