using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Accounts.Commands.LoginPassword;

public class LoginHandler : ICommandHandler<LoginCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginHandler> _logger;
    private readonly ICustomerBuildingRepository _customerBuildingRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;

    public LoginHandler(IAccountRepository accountRepository, IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork, ILogger<LoginHandler> logger, ICustomerBuildingRepository customerBuildingRepository,
        IShopRepository shopRepository, IShopDeliveryStaffRepository shopDeliveryStaffRepository)
    {
        _accountRepository = accountRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _customerBuildingRepository = customerBuildingRepository;
        _shopRepository = shopRepository;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
    }

    public async Task<Result<Result>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var account = _accountRepository.GetAccountByEmail(request.Email);
        if (account == null || !BCrypUnitls.Verify(request.Password, account.Password))
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_USERNAME_PASSWORD.GetDescription(), HttpStatusCode.Unauthorized);
        }
        else if (request.LoginContext == LoginContextType.AppForUser && account.RoleId != (int)Domain.Enums.Roles.Customer)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_ROLE.GetDescription());
        }
        else if (request.LoginContext == LoginContextType.AppForShopOrDelivery && account.RoleId != (int)Domain.Enums.Roles.ShopOwner && account.RoleId != (int)Domain.Enums.Roles.ShopDelivery)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_ROLE.GetDescription());
        }
        else if (request.LoginContext == LoginContextType.WebForShop && account.RoleId != (int)Domain.Enums.Roles.ShopOwner)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_ROLE.GetDescription());
        }
        else if (request.LoginContext == LoginContextType.WebForAdminOrModerator && account.RoleId != (int)Domain.Enums.Roles.Admin && account.RoleId != (int)Domain.Enums.Roles.Moderator)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_ROLE.GetDescription());
        }
        else if (account.Status == AccountStatus.UnVerify)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_UNVERIFIED.GetDescription());
        }
        else if (account.Status == AccountStatus.Banned)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_BANNED.GetDescription());
        }
        else
        {
            if (account.RoleId == (int)Domain.Enums.Roles.ShopOwner && _shopRepository.GetById(account.Id)!.Status == ShopStatus.UnApprove)
            {
                throw new InvalidBusinessException(MessageCode.E_SHOP_UN_APPROVE.GetDescription());
            }
            else if (account.RoleId == (int)Domain.Enums.Roles.ShopDelivery && !_shopDeliveryStaffRepository.CheckStaffOfShopActiveAndStaffActive(account.Id))
            {
                throw new InvalidBusinessException(MessageCode.E_SHOP_DELIVERY_STAFF_CAN_NOT_LOGIN.GetDescription());
            }
            else
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
                    Genders = account.Genders,
                };
                if (account.RoleId == (int)Domain.Enums.Roles.Customer)
                {
                    var customerBuilding = _customerBuildingRepository.GetDefaultByCustomerId(account.Id);
                    loginResponse.AccountResponse.IsSelectedBuilding = customerBuilding != default;
                    if (customerBuilding != default)
                    {
                        loginResponse.AccountResponse.Building = new AccountResponse.BuildingInAccount()
                        {
                            Id = customerBuilding.Building.Id,
                            Name = customerBuilding.Building.Name,
                        };

                        if (customerBuilding.Building.Location != null)
                        {
                            loginResponse.AccountResponse.Building.Longitude = customerBuilding.Building.Location.Longitude;
                            loginResponse.AccountResponse.Building.Latitude = customerBuilding.Building.Location.Latitude;
                        }
                    }
                }

                return Result.Success(loginResponse);
            }
        }
    }
}