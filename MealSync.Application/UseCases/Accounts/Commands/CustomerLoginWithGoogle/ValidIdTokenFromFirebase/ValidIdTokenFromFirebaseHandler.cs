using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Accounts.Commands.CustomerLoginWithGoogle.ValidIdTokenFromFirebase;

public class ValidIdTokenFromFirebaseHandler : ICommandHandler<ValidIdTokenFromFirebaseCommand, Result>
{
    private readonly IFirebaseAuthenticateService _firebaseAuthenticateService;
    private readonly ICacheService _cacheService;
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ValidIdTokenFromFirebaseHandler> _logger;

    private const string KEY_CACHE = "{0}-Code-Login-GG";

    public ValidIdTokenFromFirebaseHandler(IFirebaseAuthenticateService firebaseAuthenticateService, ICacheService cacheService, IAccountRepository accountRepository, IJwtTokenService jwtTokenService, IUnitOfWork unitOfWork, ILogger<ValidIdTokenFromFirebaseHandler> logger)
    {
        _firebaseAuthenticateService = firebaseAuthenticateService;
        _cacheService = cacheService;
        _accountRepository = accountRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(ValidIdTokenFromFirebaseCommand request, CancellationToken cancellationToken)
    {
        var firebaseUser = await _firebaseAuthenticateService.GetFirebaseUserAsync(request.IdToken).ConfigureAwait(false);
        if (firebaseUser != null)
        {
            var account = _accountRepository.GetAccountByEmail(firebaseUser.Email);

            // 1.1 Return an error if the account exist and verify will by pass login
            if (account != null && account.Status == AccountStatus.Verify && account.RoleId == (int)Domain.Enums.Roles.Customer)
            {
                var accessToken = _jwtTokenService.GenerateJwtToken(account);
                var refreshToken = _jwtTokenService.GenerateJwtToken(account);
                try
                {
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                    account.RefreshToken = refreshToken;
                    _accountRepository.Update(account);
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _unitOfWork.RollbackTransaction();
                    _logger.LogError(e, e.Message);
                    throw new("Internal Server Error");
                }

                LoginResponse loginResponse = new LoginResponse();
                loginResponse.TokenResponse = new TokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
                loginResponse.AccountResponse = new AccountResponse
                {
                    Id = account.Id,
                    Email = account.Email,
                    RoleId = account.RoleId,
                    RoleName = account.Role.Name,
                    AvatarUrl = account.AvatarUrl,
                    FullName = account.FullName,
                };
                return Result.Success(loginResponse);
            }
            else if (account == null || account.Status == AccountStatus.UnVerify && account.RoleId == (int)Domain.Enums.Roles.Customer)
            {
                var code = await _cacheService.GenerateAndSetCodeToCacheAsync(string.Format(KEY_CACHE, firebaseUser.Email), RedisConstant.TIME_SECOND_EXPIRE_CODE_LOGIN_GOOGLE).ConfigureAwait(false);

                return Result.Warning(new
                {
                    CodeConfirm = code,
                    Email = firebaseUser.Email,
                    FUserId = firebaseUser.UserId,
                    AvatarUrl = firebaseUser.Picture,
                    Name = firebaseUser.Name,
                });
            }
            else if (account.Status == AccountStatus.Banned)
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_BANNED.GetDescription());
            }
            else
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_HAVE_REGISTER_ACCOUNT_BUT_GOT_ISSUE.GetDescription(), new object[] { firebaseUser.Email });
            }
        }
        else
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_ID_TOKEN_NOT_VALID.GetDescription());
        }
    }
}