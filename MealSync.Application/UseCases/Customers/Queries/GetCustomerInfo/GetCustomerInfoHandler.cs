using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Customers.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Customers.Queries.GetCustomerInfo;

public class GetCustomerInfoHandler : IQueryHandler<GetCustomerInfoQuery, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ICustomerBuildingRepository _customerBuildingRepository;
    private readonly IMapper _mapper;

    public GetCustomerInfoHandler(
        IAccountRepository accountRepository, ICurrentPrincipalService currentPrincipalService,
        IMapper mapper, ICustomerBuildingRepository customerBuildingRepository)
    {
        _accountRepository = accountRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
        _customerBuildingRepository = customerBuildingRepository;
    }

    public async Task<Result<Result>> Handle(GetCustomerInfoQuery request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var customer = _accountRepository.GetCustomerById(accountId);
        var customerBuilding = _customerBuildingRepository.GetDefaultByCustomerId(accountId);

        if (customer == default)
        {
            throw new InvalidBusinessException(MessageCode.E_CUSTOMER_NOT_FOUND.GetDescription(), new object[] { accountId });
        }
        else if (customerBuilding == default)
        {
            throw new InvalidBusinessException(
                MessageCode.E_BUILDING_NOT_SELECT.GetDescription()
            );
        }
        else
        {
            var result = _mapper.Map<CustomerInfoResponse>(customer);
            result.Building = new CustomerInfoResponse.BuildingResponse
            {
                Id = customerBuilding.BuildingId,
                Name = customerBuilding.Building.Name,
            };

            if (customerBuilding.Building.Location != null)
            {
                result.Building.Longitude = customerBuilding.Building.Location.Longitude;
                result.Building.Latitude = customerBuilding.Building.Location.Latitude;
            }

            return Result.Success(result);
        }
    }
}