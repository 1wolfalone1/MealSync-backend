using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.OptionGroups.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.OptionGroups.Queries.GetListOptionGroupWithFoodLinkStatus;

public class GetListOptionGroupWithFoodLinkStatusHandler : IQueryHandler<GetListOptionGroupWithFoodLinkStatusQuery, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly IMapper _mapper;
    private readonly IFoodRepository _foodRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetListOptionGroupWithFoodLinkStatusHandler(IUnitOfWork unitOfWork, IOptionGroupRepository optionGroupRepository, IMapper mapper, IFoodRepository foodRepository, ICurrentPrincipalService currentPrincipalService)
    {
        _unitOfWork = unitOfWork;
        _optionGroupRepository = optionGroupRepository;
        _mapper = mapper;
        _foodRepository = foodRepository;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetListOptionGroupWithFoodLinkStatusQuery request, CancellationToken cancellationToken)
    {
        // Validate
        await ValidateAsync(request).ConfigureAwait(false);

        var optionGroup = _optionGroupRepository.GetOptionGroupsWithFoodLinkStatus(_currentPrincipalService.CurrentPrincipalId.Value, request.FoodId, request.FilterMode);
        return Result.Success(_mapper.Map<List<ShopOptionGroupLinkeFoodStatusResponse>>(optionGroup));
    }

    private async Task ValidateAsync(GetListOptionGroupWithFoodLinkStatusQuery request)
    {
        // Check existed food
        var existedFood = await _foodRepository.CheckExistedByIdAndShopId(request.FoodId, _currentPrincipalService.CurrentPrincipalId.Value).ConfigureAwait(false);
        if (!existedFood)
        {
            throw new InvalidBusinessException(
                MessageCode.E_FOOD_NOT_FOUND.GetDescription(),
                new object[] { request.FoodId }
            );
        }
    }
}