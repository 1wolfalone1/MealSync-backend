using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Shared;

namespace MealSync.Application.UseCases.Accounts.Commands.LoginPassword;

public class LoginHandler : ICommandHandler<LoginCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerBuildingRepository _customerBuildingRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginHandler(IAccountRepository accountRepository, ICustomerBuildingRepository customerBuildingRepository,
        IJwtTokenService jwtTokenService, IBuildingRepository buildingRepository,
        ISystemResourceRepository systemResourceRepository)
    {
        _accountRepository = accountRepository;
        _customerBuildingRepository = customerBuildingRepository;
        _jwtTokenService = jwtTokenService;
        _buildingRepository = buildingRepository;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var account = _accountRepository.GetAccountByEmail(request.Email);
        if (account == null || !BCrypUnitls.Verify(request.Password, account.Password))
        {
            return Result.Failure(new Error(StatusCode.UNAUTHORIZED.GetDescription(),
                _systemResourceRepository.GetByResourceCode(MessageCode.BU00001.ToString())));
        }
        else if (account.Status == AccountStatus.UnVerify)
        {
            return Result.Failure(new Error(StatusCode.UNAUTHORIZED.GetDescription(),
                _systemResourceRepository.GetByResourceCode(MessageCode.BU00002.ToString())));
        }
        else if (account.Status == AccountStatus.Ban)
        {
            return Result.Failure(new Error(StatusCode.UNAUTHORIZED.GetDescription(),
                _systemResourceRepository.GetByResourceCode(MessageCode.BU00003.ToString())));
        }
        else
        {
            var token = _jwtTokenService.GenerateJwtToken(account);
            var customerBuilding = _customerBuildingRepository.GetDefaultByCustomerId(account.Id);
            BuildingResponse? buildingResponse = null;
            if (customerBuilding != null)
            {
                var building = _buildingRepository.GetBuildingById(customerBuilding.BuildingId);
                buildingResponse = new BuildingResponse
                {
                    Id = building.Id,
                    Name = building.Name,
                    Latitude = building.Location.Latitude,
                    Longitude = building.Location.Longitude,
                    Dormitory = new DormitoryResponse
                    {
                        Id = building.Dormitory.Id,
                        Name = building.Dormitory.Name,
                        Latitude = building.Dormitory.Location.Latitude,
                        Longitude = building.Dormitory.Location.Longitude,
                    }
                };
            }

            LoginResponse loginResponse = new LoginResponse();
            loginResponse.TokenResponse = new TokenResponse
            {
                AccessToken = token
            };
            loginResponse.AccountResponse = new AccountResponse
            {
                Id = account.Id,
                Email = account.Email,
                RoleName = account.Role.Name,
                AvatarUrl = account.AvatarUrl,
                FullName = account.FullName,
                Building = buildingResponse
            };
            return Result.Success(loginResponse);
        }
    }
}