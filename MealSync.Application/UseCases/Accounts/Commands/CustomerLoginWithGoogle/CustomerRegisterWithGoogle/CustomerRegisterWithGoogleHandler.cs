using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Accounts.Commands.CustomerLoginWithGoogle.CustomerRegisterWithGoogle;

public class CustomerRegisterWithGoogleHandler : ICommandHandler<CustomerRegisterWithGoogleCommand, Result>
{
    private readonly ICacheService _cacheService;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerRegisterWithGoogleCommand> _logger;
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICustomerBuildingRepository _customerBuildingRepository;

    private const string KEY_CACHE = "{0}-Code-Login-GG";

    public CustomerRegisterWithGoogleHandler(ICacheService cacheService, IAccountRepository accountRepository, IUnitOfWork unitOfWork, ILogger<CustomerRegisterWithGoogleCommand> logger, IBuildingRepository buildingRepository, ICustomerRepository customerRepository, ISystemResourceRepository systemResourceRepository, IJwtTokenService jwtTokenService, ICustomerBuildingRepository customerBuildingRepository)
    {
        _cacheService = cacheService;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _buildingRepository = buildingRepository;
        _customerRepository = customerRepository;
        _systemResourceRepository = systemResourceRepository;
        _jwtTokenService = jwtTokenService;
        _customerBuildingRepository = customerBuildingRepository;
    }

    public async Task<Result<Result>> Handle(CustomerRegisterWithGoogleCommand request, CancellationToken cancellationToken)
    {
        // Validate
        await ValidateAsync(request).ConfigureAwait(false);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var account = _accountRepository.GetAccountByEmail(request.Email);

            if (account != null)
            {
                // If account exist but not verify yet

                if (_accountRepository.CheckExistPhoneNumberInOtherEmailAccount(request.Email, request.PhoneNumber))
                {
                    throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription());
                }

                account.PhoneNumber = request.PhoneNumber;
                account.Status = AccountStatus.Verify;
                account.DeviceToken = request.DeviceToken;
                account.FUserId = request.FUserId;

                var customerBuilding = new CustomerBuilding()
                {
                    BuildingId = request.BuildingId,
                    CustomerId = account.Id,
                    IsDefault = true,
                };

                var customer = _customerRepository.GetById(account.Id);
                customer.CustomerBuildings = new List<CustomerBuilding>()
                {
                    customerBuilding,
                };
                customer.Status = CustomerStatus.Active;
                _accountRepository.Update(account);
                _customerRepository.Update(customer);
            }
            else
            {
                // If account exist but not verify yet
                if (_accountRepository.CheckExistByPhoneNumber(request.PhoneNumber))
                {
                    throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription());
                }

                var customerBuilding = new CustomerBuilding()
                {
                    BuildingId = request.BuildingId,
                    IsDefault = true,
                };

                var customer = new Customer()
                {
                    Status = CustomerStatus.Active,
                    CustomerBuildings = new List<CustomerBuilding>()
                    {
                        customerBuilding,
                    },
                };

                account = new Account()
                {
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    AvatarUrl = !string.IsNullOrEmpty(request.AvatarUrl) ? request.AvatarUrl : _systemResourceRepository.GetByResourceCode(ResourceCode.ACCOUNT_AVATAR.GetDescription()),
                    Genders = Genders.UnKnown,
                    FullName = request.Name,
                    Type = AccountTypes.Google,
                    FUserId = request.FUserId,
                    DeviceToken = request.DeviceToken,
                    Status = AccountStatus.Verify,
                    RoleId = (int)Domain.Enums.Roles.Customer,
                    Customer = customer,
                    Password = string.Empty,
                };

                await _accountRepository.AddAsync(account).ConfigureAwait(false);
            }

            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        // Update Token
        var accountGetNew = _accountRepository.GetAccountByEmail(request.Email);
        var accessToken = _jwtTokenService.GenerateJwtToken(accountGetNew);
        var refreshToken = _jwtTokenService.GenerateJwtToken(accountGetNew);
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            accountGetNew.RefreshToken = refreshToken;
            _accountRepository.Update(accountGetNew);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }

        var accountResponse = _accountRepository.GetAccountByEmail(request.Email);
        LoginResponse loginResponse = new LoginResponse();
        loginResponse.TokenResponse = new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
        loginResponse.AccountResponse = new AccountResponse
        {
            Id = accountResponse.Id,
            Email = accountResponse.Email,
            RoleId = accountResponse.RoleId,
            RoleName = accountResponse.Role.Name,
            AvatarUrl = accountResponse.AvatarUrl,
            FullName = accountResponse.FullName,
            PhoneNumber = accountResponse.PhoneNumber,
        };

        var customerBuildingTemp = _customerBuildingRepository.GetDefaultByCustomerId(accountResponse.Id);
        loginResponse.AccountResponse.IsSelectedBuilding = customerBuildingTemp != default;
        if (customerBuildingTemp != default)
        {
            loginResponse.AccountResponse.Building = new AccountResponse.BuildingInAccount()
            {
                Id = customerBuildingTemp.Building.Id,
                Name = customerBuildingTemp.Building.Name,
            };

            if (customerBuildingTemp.Building.Location != null)
            {
                loginResponse.AccountResponse.Building.Longitude = customerBuildingTemp.Building.Location.Longitude;
                loginResponse.AccountResponse.Building.Latitude = customerBuildingTemp.Building.Location.Latitude;
            }
        }

        return Result.Success(loginResponse);
    }

    private async Task ValidateAsync(CustomerRegisterWithGoogleCommand request)
    {
        var codeInCache = await _cacheService.GetCachedResponseAsync(string.Format(KEY_CACHE, request.Email)).ConfigureAwait(false);
        if (string.IsNullOrEmpty(codeInCache))
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_CODE_NOT_FOUND_OR_OVER_TIME.GetDescription());
        }

        var account = _accountRepository.GetAccountByEmail(request.Email);
        if (account != null && (account.Status != AccountStatus.UnVerify || account.RoleId != (int)Domain.Enums.Roles.Customer))
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_EXIST.GetDescription());
        }

        var building = _buildingRepository.GetById(request.BuildingId);
        if (building == default)
        {
            throw new InvalidBusinessException(MessageCode.E_BUILDING_NOT_FOUND.GetDescription(), new object[] { request.BuildingId });
        }
    }
}