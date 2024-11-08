using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MealSync.Application.UseCases.Accounts.Commands.VerifyCode;

public class VerifyCodeHandler : ICommandHandler<VerifyCodeCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyCodeHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public VerifyCodeHandler(
        IAccountRepository accountRepository, IUnitOfWork unitOfWork,
        ILogger<VerifyCodeHandler> logger, IEmailService emailService,
        ICacheService cacheService, ISystemResourceRepository systemResourceRepository)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _emailService = emailService;
        _cacheService = cacheService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(VerifyCodeCommand request, CancellationToken cancellationToken)
    {
        var account = _accountRepository.GetAccountByEmail(request.Email);

        if (account == default)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_NOT_FOUND.GetDescription());
        }
        else if (
            account.Status != AccountStatus.UnVerify &&
            (request.VerifyType == VerifyType.CustomerRegister || request.VerifyType == VerifyType.ShopRegister))
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_ALREADY_VERIFY.GetDescription());
        }
        else if (
            (request.VerifyType == VerifyType.CustomerRegister && account.RoleId != (int)Domain.Enums.Roles.Customer)
            || (request.VerifyType == VerifyType.ShopRegister && account.RoleId != (int)Domain.Enums.Roles.ShopOwner))
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_VERIFY_INVALID_ROLE.GetDescription());
        }
        else if (account.Status == AccountStatus.UnVerify && request.VerifyType == VerifyType.ForgotPassword)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_UNVERIFIED.GetDescription());
        }
        else
        {
            var cacheKey = GenerateCacheKey(request.VerifyType, account.Email);
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

            if (request.IsVerify && (string.IsNullOrEmpty(dataCached) || dataCached != request.Code.ToString()))
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_VERIFY_CODE.GetDescription());
            }
            else if (!request.IsVerify && (string.IsNullOrEmpty(dataCached) || !BCrypUnitls.Verify(request.Code.ToString(), dataCached)))
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_VERIFY_CODE.GetDescription());
            }
            else
            {
                if (request.IsVerify && (request.VerifyType == VerifyType.CustomerRegister || request.VerifyType == VerifyType.ShopRegister))
                {
                    account.Status = AccountStatus.Verify;
                }
                else if (request.IsVerify && request.VerifyType == VerifyType.ForgotPassword)
                {
                    await _cacheService.SetCacheResponseAsync(
                        cacheKey,
                        BCrypUnitls.Hash(request.Code.ToString()),
                        TimeSpan.FromSeconds(RedisConstant.TIME_VERIFY_CODE_LIVE * 5)).ConfigureAwait(false);

                    return Result.Success(new
                    {
                        Code = MessageCode.I_ACCOUNT_VERIFY_FOR_UPDATE_PASS_SUCCESS.GetDescription(),
                        Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_VERIFY_FOR_UPDATE_PASS_SUCCESS.GetDescription()),
                    });
                }
                else if (!request.IsVerify && request.VerifyType == VerifyType.ForgotPassword)
                {
                    account.Password = BCrypUnitls.Hash(request.Password!);
                }
                else
                {
                    throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_VERIFY_CODE.GetDescription());
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                    _accountRepository.Update(account);
                    await _cacheService.RemoveCacheResponseAsync(cacheKey).ConfigureAwait(false);
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                    return Result.Success(new
                    {
                        Code = request.VerifyType != VerifyType.ForgotPassword
                            ? MessageCode.I_ACCOUNT_VERIFY_SUCCESS.GetDescription()
                            : MessageCode.I_ACCOUNT_CHANGE_PASSWORD_SUCCESS.GetDescription(),
                        Message = _systemResourceRepository.GetByResourceCode(request.VerifyType != VerifyType.ForgotPassword
                            ? MessageCode.I_ACCOUNT_VERIFY_SUCCESS.GetDescription()
                            : MessageCode.I_ACCOUNT_CHANGE_PASSWORD_SUCCESS.GetDescription()),
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

    private string GenerateCacheKey(VerifyType verifyType, string email)
    {
        if (verifyType == VerifyType.CustomerRegister || verifyType == VerifyType.ShopRegister)
        {
            return VerificationCodeTypes.Register.GetDescription() + "-" + email;
        }
        else
        {
            return VerificationCodeTypes.ForgotPassword.GetDescription() + "-" + email;
        }
    }
}