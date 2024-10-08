using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Foods.Queries.FoodDetailOfShop;

public class GetFoodDetailOfShopHandler : IQueryHandler<GetFoodDetailOfShopQuery, Result>
{
    private readonly IFoodRepository _foodRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetFoodDetailOfShopHandler(IFoodRepository foodRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _foodRepository = foodRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetFoodDetailOfShopQuery request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var existedFood = await _foodRepository.CheckExistedByIdAndShopId(request.Id, accountId).ConfigureAwait(false);

        // Check existed food
        if (!existedFood)
        {
            throw new InvalidBusinessException(
                MessageCode.E_FOOD_NOT_FOUND.GetDescription(),
                new object[] { request.Id }
            );
        }
        else
        {
            return Result.Success(_mapper.Map<FoodDetailOfShopResponse>(_foodRepository.GetByIdIncludeAllInfoForShop(request.Id)));
        }
    }
}