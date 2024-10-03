using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Foods.Queries.TopFood;

public class GetTopFoodHandler : IQueryHandler<GetTopFoodQuery, Result>
{
    private readonly IFoodRepository _foodRepository;
    private readonly ICustomerBuildingRepository _customerBuildingRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetTopFoodHandler(
        IFoodRepository foodRepository, ICustomerBuildingRepository customerBuildingRepository,
        ICurrentPrincipalService currentPrincipalService, IMapper mapper
    )
    {
        _foodRepository = foodRepository;
        _customerBuildingRepository = customerBuildingRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetTopFoodQuery request, CancellationToken cancellationToken)
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
            var topFoods = await _foodRepository.GetTopFood(
                defaultBuilding.Building.DormitoryId, request.PageIndex, request.PageSize
            ).ConfigureAwait(false);
            var result = new PaginationResponse<FoodSummaryResponse>(
                _mapper.Map<List<FoodSummaryResponse>>(topFoods.Foods), topFoods.TotalCount, request.PageIndex, request.PageSize
            );

            return Result.Success(result);
        }
    }
}