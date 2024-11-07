using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Wallets.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Wallets.Commands.Shop.SendCodeWithdrawalRequest;

public class SendCodeWithdrawalRequestHandler : ICommandHandler<SendCodeWithdrawalRequestCommand, Result>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;

    public SendCodeWithdrawalRequestHandler(
        IWalletRepository walletRepository, IShopRepository shopRepository,
        ICurrentPrincipalService currentPrincipalService, ICacheService cacheService,
        IEmailService emailService, IAccountRepository accountRepository,
        ISystemResourceRepository systemResourceRepository, IWithdrawalRequestRepository withdrawalRequestRepository)
    {
        _walletRepository = walletRepository;
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
        _cacheService = cacheService;
        _emailService = emailService;
        _accountRepository = accountRepository;
        _systemResourceRepository = systemResourceRepository;
        _withdrawalRequestRepository = withdrawalRequestRepository;
    }

    public async Task<Result<Result>> Handle(SendCodeWithdrawalRequestCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shop = await _shopRepository.GetByAccountId(shopId).ConfigureAwait(false);
        var account = _accountRepository.GetById(shopId)!;
        var wallet = _walletRepository.GetById(shop.WalletId);
        var existingPendingRequest = await _withdrawalRequestRepository.CheckExistingPendingRequestByWalletId(wallet!.Id).ConfigureAwait(false);

        if (existingPendingRequest)
        {
            throw new InvalidBusinessException(MessageCode.E_WITHDRAWAL_REQUEST_ONLY_ONE_PENDING.GetDescription());
        }
        else if (wallet.AvailableAmount <= 0)
        {
            throw new InvalidBusinessException(MessageCode.E_WITHDRAWAL_NOT_ENOUGH_AVAILABLE_AMOUNT.GetDescription());
        }
        else if (request.Amount > wallet.AvailableAmount)
        {
            throw new InvalidBusinessException(MessageCode.E_WITHDRAWAL_AMOUNT_MUST_LESS_THAN_OR_EQUAL_AVAILABLE_AMOUNT.GetDescription(), new object[] { MoneyUtils.FormatMoneyWithDots(wallet.AvailableAmount) });
        }
        else
        {
            var withdrawalRequestCacheDto = new WithdrawalRequestCacheDto
            {
                Data = new WithdrawalRequestCacheDto.WithdrawalRequestDto
                {
                    Amount = request.Amount,
                    BankAccountNumber = request.BankAccountNumber,
                    BankCode = request.BankCode,
                    BankShortName = request.BankShortName,
                },
            };

            SendAndSaveVerificationCode(account.Email, MoneyUtils.FormatMoneyWithDots(request.Amount), withdrawalRequestCacheDto);
            return Result.Create(new
            {
                Email = account.Email,
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_WITHDRAWAL_REQUEST_SEND_MAIL_SUCCESS.GetDescription()) ?? string.Empty,
            });
        }
    }

    private void SendAndSaveVerificationCode(string email, string amount, WithdrawalRequestCacheDto withdrawalRequestCacheDtoDto)
    {
        var code = new Random().Next(1000, 10000).ToString();
        withdrawalRequestCacheDtoDto.Code = code;

        _cacheService.SetCacheResponseAsync(
            GenerateCacheKey(VerificationCodeTypes.Withdrawal, email), withdrawalRequestCacheDtoDto, TimeSpan.FromSeconds(RedisConstant.TIME_VERIFY_CODE_LIVE));

        var isSendMail = _emailService.SendVerificationCodeWithdrawalRequest(email, code, amount);

        if (!isSendMail)
        {
            throw new("Internal Server Error");
        }
    }

    private string GenerateCacheKey(VerificationCodeTypes verificationCodeType, string email)
    {
        return verificationCodeType.GetDescription() + "-" + email;
    }
}