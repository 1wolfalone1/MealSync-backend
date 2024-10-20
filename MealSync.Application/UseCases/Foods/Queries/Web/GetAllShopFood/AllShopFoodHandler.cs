using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using Org.BouncyCastle.Ocsp;

namespace MealSync.Application.UseCases.Foods.Queries.Web.GetAllShopFood;

public class AllShopFoodHandler : IQueryHandler<AllShopFoodQuery, Result>
{
    private readonly IFoodRepository _foodRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public AllShopFoodHandler(IFoodRepository foodRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService)
    {
        _foodRepository = foodRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(AllShopFoodQuery request, CancellationToken cancellationToken)
    {
        var shopFoods = await _foodRepository.GetAllShopFoodForWeb(_currentPrincipalService.CurrentPrincipalId.Value, request.PageIndex, request.PageSize, request.StatusMode, request.OperatingSlotId, request.Name)
            .ConfigureAwait(false);

        var response = new PaginationResponse<ShopFoodWebResponse>(_mapper.Map<List<ShopFoodWebResponse>>(shopFoods.Foods), shopFoods.TotalCount, request.PageIndex, request.PageSize);
        return Result.Success(response);
    }
}