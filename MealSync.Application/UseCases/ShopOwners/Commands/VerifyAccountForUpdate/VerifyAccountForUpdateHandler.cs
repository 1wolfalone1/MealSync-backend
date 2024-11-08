using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Newtonsoft.Json;

namespace MealSync.Application.UseCases.ShopOwners.Commands.VerifyAccountForUpdate;

public class VerifyAccountForUpdateHandler : ICommandHandler<VerifyAccountForUpdateCommand, Result>
{
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICacheService _cacheService;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public VerifyAccountForUpdateHandler(
        ISystemResourceRepository systemResourceRepository, IAccountRepository accountRepository,
        ICacheService cacheService, ICurrentPrincipalService currentPrincipalService)
    {
        _systemResourceRepository = systemResourceRepository;
        _accountRepository = accountRepository;
        _cacheService = cacheService;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(VerifyAccountForUpdateCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shopAccount = _accountRepository.GetById(shopId)!;

        string cacheKey = GenerateCacheKey(VerificationCodeTypes.VerifyOldEmail, shopAccount.Email);

        var cachedValue = await _cacheService.GetCachedResponseAsync(cacheKey).ConfigureAwait(false);
        string? dataCached;

        if (cachedValue == default || cachedValue.Length == 0)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_VERIFY_CODE.GetDescription());
        }
        else
        {
            dataCached = JsonConvert.DeserializeObject<string>(cachedValue);
        }

        if (string.IsNullOrEmpty(dataCached) || dataCached != request.Code.ToString())
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_VERIFY_CODE.GetDescription());
        }
        else
        {
            await _cacheService.SetCacheResponseAsync(
                cacheKey,
                BCrypUnitls.Hash(dataCached),
                TimeSpan.FromSeconds(RedisConstant.TIME_VERIFY_CODE_LIVE * 5)).ConfigureAwait(false);

            return Result.Success(new
            {
                Code = MessageCode.I_ACCOUNT_VERIFY_OLD_EMAIL_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_VERIFY_OLD_EMAIL_SUCCESS.GetDescription()),
            });
        }
    }

    private string GenerateCacheKey(VerificationCodeTypes verificationCodeType, string email)
    {
        return verificationCodeType.GetDescription() + "-" + email;
    }
}