using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Accounts.Commands.SignupCustomer;

public class SignupCustomerHandler : ICommandHandler<SignupCustomerCommand, Result>
{
    private readonly ILogger<SignupCustomerHandler> _logger;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAccountRepository _accountRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ICacheService _cacheService;

    public SignupCustomerHandler(
        ILogger<SignupCustomerHandler> logger, IJwtTokenService jwtTokenService,
        IAccountRepository accountRepository, IEmailService emailService, IUnitOfWork unitOfWork,
        ISystemResourceRepository systemResourceRepository, ICacheService cacheService
    )
    {
        _logger = logger;
        _jwtTokenService = jwtTokenService;
        _accountRepository = accountRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _systemResourceRepository = systemResourceRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<Result>> Handle(SignupCustomerCommand request, CancellationToken cancellationToken)
    {
        var account = _accountRepository.GetAccountByEmail(request.Email);

        // 1. Existed account
        if (account != null)
        {
            // 1.1 Return an error if the account exists and its status is not unverified or not role customer.
            if (account.Status != AccountStatus.UnVerify || account.RoleId != (int)Domain.Enums.Roles.Customer)
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_EXIST.GetDescription());
            }

            // 1.2 Return an error if the phone number exists.
            if (account.PhoneNumber != request.PhoneNumber && _accountRepository.CheckExistByPhoneNumber(request.PhoneNumber))
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription());
            }

            // 1.3 Update the account and resend verification code.
            account.PhoneNumber = request.PhoneNumber;
            account.Password = BCrypUnitls.Hash(request.Password);
            var refreshToken = _jwtTokenService.GenerateJwtRefreshToken(account);
            account.RefreshToken = refreshToken;
            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                _accountRepository.Update(account);
                SendAndSaveVerificationCode(request.Email);
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw new("Internal Server Error");
            }
            return Result.Create(new RegisterResponse
            {
                Email = account.Email,
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_REGISTER_SUCCESSFULLY.GetDescription()) ?? string.Empty,
            });
        }
        else
        {
            // 2. Not exist account
            // 2.1 Return an error if the phone number exists.
            if (_accountRepository.CheckExistByPhoneNumber(request.PhoneNumber))
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription());
            }

            // 2.2 Create a new account.
            var newAccount = new Account
            {
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypUnitls.Hash(request.Password),
                AvatarUrl = _systemResourceRepository.GetByResourceCode(ResourceCode.ACCOUNT_AVATAR.GetDescription()) ?? string.Empty,
                FullName = string.Empty,
                RoleId = (int)Domain.Enums.Roles.Customer,
                Type = AccountTypes.Local,
                Status = AccountStatus.UnVerify,
            };

            var refreshToken = _jwtTokenService.GenerateJwtToken(newAccount);
            newAccount.RefreshToken = refreshToken;
            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                await _accountRepository.AddAsync(newAccount);
                SendAndSaveVerificationCode(request.Email);
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw new("Internal Server Error");
            }
            return Result.Create(new RegisterResponse
            {
                Email = request.Email,
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_REGISTER_SUCCESSFULLY.GetDescription()) ?? string.Empty,
            });
        }
    }

    private void SendAndSaveVerificationCode(string email)
    {
        var code = new Random().Next(1000, 10000).ToString();
        _cacheService.SetCacheResponseAsync(
            GenerateCacheKey(VerificationCodeTypes.Register, email),
            code, TimeSpan.FromSeconds(RedisConstant.TIME_VERIFY_CODE_LIVE));

        var isSendMail = _emailService.SendVerificationCodeRegister(email, code);
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