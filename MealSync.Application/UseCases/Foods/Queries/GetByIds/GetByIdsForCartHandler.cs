using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Queries.GetByIds;

public class GetByIdsForCartHandler : IQueryHandler<GetByIdsForCartQuery, Result>
{
    private readonly IFoodRepository _foodRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IMapper _mapper;

    public GetByIdsForCartHandler(IFoodRepository foodRepository, ISystemResourceRepository systemResourceRepository, IMapper mapper)
    {
        _foodRepository = foodRepository;
        _systemResourceRepository = systemResourceRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetByIdsForCartQuery request, CancellationToken cancellationToken)
    {
        var data = await _foodRepository.GetByIds(request.Ids).ConfigureAwait(false);
        var foods = _mapper.Map<List<FoodCartSummaryResponse>>(data.Foods);

        return Result.Success(new
        {
            Foods = foods,
            IsHaveNotFound = data.IdsNotFound.Count > 0,
            IdsNotFound = data.IdsNotFound.Count > 0 ? data.IdsNotFound : null,
            Message = data.IdsNotFound.Count > 0
                ? _systemResourceRepository.GetByResourceCode(
                    MessageCode.E_FOOD_CART_NOT_FOUND.GetDescription(), data.IdsNotFound.Count)
                : null,
        });
    }
}