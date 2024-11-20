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

namespace MealSync.Application.UseCases.Foods.Queries.FoodOfShopForModManage;

public class GetFoodOfShopForModManageHandler : IQueryHandler<GetFoodOfShopForModManageQuery, Result>
{
    private readonly IFoodRepository _foodRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IShopDormitoryRepository _shopDormitoryRepository;
    private readonly IMapper _mapper;

    public GetFoodOfShopForModManageHandler(
        IFoodRepository foodRepository, ICurrentPrincipalService currentPrincipalService,
        IModeratorDormitoryRepository moderatorDormitoryRepository, IShopDormitoryRepository shopDormitoryRepository, IMapper mapper)
    {
        _foodRepository = foodRepository;
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _shopDormitoryRepository = shopDormitoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetFoodOfShopForModManageQuery request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();
        var isExistedInDormitoryManage = await _shopDormitoryRepository.CheckShopDormitory(request.ShopId, dormitoryIds).ConfigureAwait(false);

        if (isExistedInDormitoryManage)
        {
            var data = await _foodRepository.GetShopFoodForModManage(request.ShopId, request.PageIndex, request.PageSize).ConfigureAwait(false);
            var result = new PaginationResponse<FoodDetailForModManageResponse>(
                _mapper.Map<List<FoodDetailForModManageResponse>>(data.Foods),
                data.TotalCount,
                request.PageIndex,
                request.PageSize);
            return Result.Success(result);
        }
        else
        {
            throw new InvalidBusinessException(MessageCode.E_MODERATOR_NOT_HAVE_MANAGE_PERMISSION.GetDescription());
        }
    }
}