using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateEmail;

public class UpdateEmailHandler : ICommandHandler<UpdateEmailCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ICacheService _cacheService;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateEmailHandler> _logger;
    private readonly IJwtTokenService _jwtTokenService;

    public UpdateEmailHandler(
        IAccountRepository accountRepository, ISystemResourceRepository systemResourceRepository,
        ICacheService cacheService, ICurrentPrincipalService currentPrincipalService,
        IUnitOfWork unitOfWork, ILogger<UpdateEmailHandler> logger, IJwtTokenService jwtTokenService)
    {
        _accountRepository = accountRepository;
        _systemResourceRepository = systemResourceRepository;
        _cacheService = cacheService;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<Result>> Handle(UpdateEmailCommand request, CancellationToken cancellationToken)
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

                var codeOldEmail = JsonConvert.DeserializeObject<string>(cachedValueOldEmail);
                var codeUpdateEmail = JsonConvert.DeserializeObject<string>(cachedValueUpdateEmail);

                if (codeOldEmail == default || !BCrypUnitls.Verify(request.CodeVerifyOldEmail.ToString(), codeOldEmail))
                {
                    throw new InvalidBusinessException(MessageCode.E_ACCOUNT_UPDATE_EMAIL_OVERDUE.GetDescription());
                }
                else if (codeUpdateEmail == default || codeUpdateEmail != request.CodeVerifyNewEmail.ToString())
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
                    }
                    catch (Exception e)
                    {
                        _unitOfWork.RollbackTransaction();
                        _logger.LogError(e, e.Message);
                        throw new("Internal Server Error");
                    }

                    var accessToken = _jwtTokenService.GenerateJwtToken(shopAccount);
                    var refreshToken = _jwtTokenService.GenerateJwtToken(shopAccount);
                    LoginResponse loginResponse = new LoginResponse();
                    loginResponse.TokenResponse = new TokenResponse
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    };
                    loginResponse.AccountResponse = new AccountResponse
                    {
                        Id = shopAccount.Id,
                        Email = shopAccount.Email,
                        RoleName = shopAccount.Role.Name,
                        AvatarUrl = shopAccount.AvatarUrl,
                        FullName = shopAccount.FullName,
                    };
                    return Result.Success(new
                    {
                        Code = MessageCode.I_ACCOUNT_UPDATE_EMAIL_SUCCESS.GetDescription(),
                        Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_UPDATE_EMAIL_SUCCESS.GetDescription()),
                        TokenAndInfor = loginResponse,
                    });
                }
            }
        }
    }

    private string GenerateCacheKey(VerificationCodeTypes verificationCodeType, string email)
    {
        return verificationCodeType.GetDescription() + "-" + email;
    }
}