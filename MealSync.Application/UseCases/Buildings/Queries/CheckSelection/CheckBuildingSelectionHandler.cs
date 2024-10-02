using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Buildings.Queries.CheckSelection;

public class CheckBuildingSelectionHandler : IQueryHandler<CheckBuildingSelectionQuery, Result>
{
    private readonly ICustomerBuildingRepository _customerBuildingRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public CheckBuildingSelectionHandler(
        ICustomerBuildingRepository customerBuildingRepository, ICurrentPrincipalService currentPrincipalService,
        ISystemResourceRepository systemResourceRepository
    )
    {
        _customerBuildingRepository = customerBuildingRepository;
        _currentPrincipalService = currentPrincipalService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(CheckBuildingSelectionQuery request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var defaultBuilding = _customerBuildingRepository.GetDefaultByCustomerId(accountId);

        // Throw error when customer has not selected dormitory
        if (defaultBuilding == null)
        {
            throw new InvalidBusinessException(
                MessageCode.E_BUILDING_NOT_SELECT.GetDescription()
            );
        }
        else
        {
            return Result.Success(new
            {
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.E_BUILDING_NOT_SELECT.GetDescription()),
            });
        }
    }
}