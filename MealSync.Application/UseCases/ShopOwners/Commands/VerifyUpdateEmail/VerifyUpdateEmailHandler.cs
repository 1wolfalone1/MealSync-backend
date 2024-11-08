using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MealSync.Application.UseCases.ShopOwners.Commands.VerifyUpdateEmail;

public class VerifyUpdateEmailHandler : ICommandHandler<VerifyUpdateEmailCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ICacheService _cacheService;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyUpdateEmailHandler> _logger;

    public VerifyUpdateEmailHandler(
        IAccountRepository accountRepository, ISystemResourceRepository systemResourceRepository,
        ICacheService cacheService, ICurrentPrincipalService currentPrincipalService,
        IUnitOfWork unitOfWork, ILogger<VerifyUpdateEmailHandler> logger)
    {
        _accountRepository = accountRepository;
        _systemResourceRepository = systemResourceRepository;
        _cacheService = cacheService;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(VerifyUpdateEmailCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shopAccount = _accountRepository.GetById(shopId)!;

        if (shopAccount.Email == request.NewEmail)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_UPDATE_MUST_DIFFER_PRESENT.GetDescription());
        }
        else
        {
            var accountCheck = _accountRepository.GetAccountByEmail(request.NewEmail);
            if (accountCheck != null)
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_EXIST.GetDescription());
            }
            else
            {
                var cacheKeyOldEmail = GenerateCacheKey(VerificationCodeTypes.VerifyOldEmail, shopAccount.Email);
                var cachedValueOldEmail = await _cacheService.GetCachedResponseAsync(cacheKeyOldEmail).ConfigureAwait(false);

                var cacheKeyUpdateEmail = GenerateCacheKey(VerificationCodeTypes.UpdateEmail, request.NewEmail);
                var cachedValueUpdateEmail = await _cacheService.GetCachedResponseAsync(cacheKeyUpdateEmail).ConfigureAwait(false);

                if (cachedValueUpdateEmail == default || cachedValueUpdateEmail.Length == 0 || cachedValueOldEmail == default || cachedValueOldEmail.Length == 0)
                {
                    throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_VERIFY_CODE.GetDescription());
                }

                var verifyOldEmailDto = JsonConvert.DeserializeObject<VerifyUpdateEmailDto>(cachedValueOldEmail);
                var verifyUpdateEmailDto = JsonConvert.DeserializeObject<VerifyUpdateEmailDto>(cachedValueUpdateEmail);

                if (
                    verifyOldEmailDto == default || verifyOldEmailDto.Code != request.CodeVerifyOldEmail || verifyOldEmailDto.Email != shopAccount.Email ||
                    verifyUpdateEmailDto == default || verifyUpdateEmailDto.Code != request.CodeVerifyNewEmail || verifyUpdateEmailDto.Email != request.NewEmail)
                {
                    throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_VERIFY_CODE.GetDescription());
                }
                else
                {

                    try
                    {
                        await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                        shopAccount.Email = request.NewEmail;
                        _accountRepository.Update(shopAccount);
                        await _cacheService.RemoveCacheResponseAsync(cacheKeyUpdateEmail).ConfigureAwait(false);
                        await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                        return Result.Success(new
                        {
                            Code = MessageCode.I_ACCOUNT_UPDATE_EMAIL_SUCCESS.GetDescription(),
                            Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_UPDATE_EMAIL_SUCCESS.GetDescription()),
                        });
                    }
                    catch (Exception e)
                    {
                        _unitOfWork.RollbackTransaction();
                        _logger.LogError(e, e.Message);
                        throw new("Internal Server Error");
                    }
                }
            }
        }
    }

    private string GenerateCacheKey(VerificationCodeTypes verificationCodeType, string email)
    {
        return verificationCodeType.GetDescription() + "-" + email;
    }
}