using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Accounts.Commands.SendVerifyCode;

public class SendVerifyCodeHandler : ICommandHandler<SendVerifyCodeCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public SendVerifyCodeHandler(
        IAccountRepository accountRepository, IEmailService emailService,
        ICacheService cacheService, ISystemResourceRepository systemResourceRepository)
    {
        _accountRepository = accountRepository;
        _emailService = emailService;
        _cacheService = cacheService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(SendVerifyCodeCommand request, CancellationToken cancellationToken)
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
            if (request.VerifyType == VerifyType.CustomerRegister || request.VerifyType == VerifyType.ShopRegister)
            {
                SendVerificationCode(request.Email, false);
            }
            else
            {
                SendVerificationCode(request.Email, true);
            }

            return Result.Success(new
            {
                Code = MessageCode.I_ACCOUNT_SEND_VERIFY_CODE_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_SEND_VERIFY_CODE_SUCCESS.GetDescription()),
            });
        }
    }

    private void SendVerificationCode(string email, bool isForgotPassword)
    {
        var code = new Random().Next(1000, 10000).ToString();
        _cacheService.SetCacheResponseAsync(
            GenerateCacheKey(isForgotPassword ? VerificationCodeTypes.ForgotPassword : VerificationCodeTypes.Register, email),
            code,
            TimeSpan.FromSeconds(RedisConstant.TIME_VERIFY_CODE_LIVE));

        var isSendMail = isForgotPassword
            ? _emailService.SendVerificationCodeForgotPassword(email, code)
            : _emailService.SendVerificationCodeRegister(email, code);
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