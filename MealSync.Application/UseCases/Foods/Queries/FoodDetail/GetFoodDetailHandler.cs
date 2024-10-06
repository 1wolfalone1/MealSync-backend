using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Foods.Queries.FoodDetail;

public class GetFoodDetailHandler : IQueryHandler<GetFoodDetailQuery, Result>
{
    private readonly IFoodRepository _foodRepository;
    private readonly IMapper _mapper;

    public GetFoodDetailHandler(IFoodRepository foodRepository, IMapper mapper)
    {
        _foodRepository = foodRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetFoodDetailQuery request, CancellationToken cancellationToken)
    {
        var existed = await _foodRepository.CheckExistedAndActiveById(request.Id).ConfigureAwait(false);
        if (existed)
        {
            return Result.Success(_mapper.Map<FoodDetailResponse>(_foodRepository.GetByIdIncludeAllInfo(request.Id)));
        }
        else
        {
            throw new InvalidBusinessException(MessageCode.E_FOOD_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
    }
}