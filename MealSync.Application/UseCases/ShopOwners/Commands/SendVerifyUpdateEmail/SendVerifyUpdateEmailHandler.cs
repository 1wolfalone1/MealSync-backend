using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.ShopOwners.Commands.SendVerifyUpdateEmail;

public class SendVerifyUpdateEmailHandler : ICommandHandler<SendVerifyUpdateEmailCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public SendVerifyUpdateEmailHandler(
        IAccountRepository accountRepository, ISystemResourceRepository systemResourceRepository,
        IEmailService emailService, ICacheService cacheService, ICurrentPrincipalService currentPrincipalService)
    {
        _accountRepository = accountRepository;
        _systemResourceRepository = systemResourceRepository;
        _emailService = emailService;
        _cacheService = cacheService;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(SendVerifyUpdateEmailCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shopAccount = _accountRepository.GetById(shopId)!;

        if (request.IsVerifyOldEmail)
        {
            SendVerificationCode(shopAccount.Email, request.NewEmail, true);
        }
        else
        {
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
                    SendVerificationCode(shopAccount.Email, request.NewEmail, false);
                }
            }
        }

        return Result.Success(new
        {
            Code = request.IsVerifyOldEmail ? MessageCode.I_ACCOUNT_SEND_VERIFY_CODE_OLD_EMAIL_SUCCESS.GetDescription() : MessageCode.I_ACCOUNT_SEND_VERIFY_CODE_UPDATE_EMAIL_SUCCESS.GetDescription(),
            Message = request.IsVerifyOldEmail
                ? _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_SEND_VERIFY_CODE_OLD_EMAIL_SUCCESS.GetDescription())
                : _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_SEND_VERIFY_CODE_UPDATE_EMAIL_SUCCESS.GetDescription()),
        });
    }

    private void SendVerificationCode(string oldEmail, string newEmail, bool isVerifyOldEmail)
    {
        var code = new Random().Next(1000, 10000).ToString();

        _cacheService.SetCacheResponseAsync(
            GenerateCacheKey(isVerifyOldEmail ? VerificationCodeTypes.VerifyOldEmail : VerificationCodeTypes.UpdateEmail, isVerifyOldEmail ? oldEmail : newEmail),
            code,
            TimeSpan.FromSeconds(RedisConstant.TIME_VERIFY_CODE_LIVE));

        var isSendMail = isVerifyOldEmail ? _emailService.SendVerificationCodeUpdateEmail(oldEmail, code) : _emailService.SendVerificationCodeOldEmail(newEmail, code);
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