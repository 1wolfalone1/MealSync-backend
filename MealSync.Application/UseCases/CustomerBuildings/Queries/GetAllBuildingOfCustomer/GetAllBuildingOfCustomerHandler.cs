using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.CustomerBuildings.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.CustomerBuildings.Queries.GetAllBuildingOfCustomer;

public class GetAllBuildingOfCustomerHandler : IQueryHandler<GetAllBuildingOfCustomerQuery, Result>
{
    private readonly ICustomerBuildingRepository _customerBuildingRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetAllBuildingOfCustomerHandler(
        ICustomerBuildingRepository customerBuildingRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _customerBuildingRepository = customerBuildingRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetAllBuildingOfCustomerQuery request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var customerBuilding = await _customerBuildingRepository.GetByCustomerIdIncludeBuildingAndDormitory(accountId).ConfigureAwait(false);

        if (customerBuilding.Count == 0)
        {
            throw new InvalidBusinessException(
                MessageCode.E_BUILDING_NOT_SELECT.GetDescription()
            );
        }
        else
        {
            return Result.Success(_mapper.Map<List<CustomerBuildingResponse>>(customerBuilding));
        }
    }
}