using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Shops.Models;

namespace MealSync.Application.UseCases.Shops.Queries.ModeratorManage.GetListShop;

public class GetListShopHandler : IQueryHandler<GetListShopQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IShopRepository _shopRepository;

    public GetListShopHandler(ICurrentPrincipalService currentPrincipalService, IModeratorDormitoryRepository moderatorDormitoryRepository, IShopRepository shopRepository)
    {
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _shopRepository = shopRepository;
    }

    public async Task<Result<Result>> Handle(GetListShopQuery request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();

        var data = await _shopRepository.GetAllShopByDormitoryIds(
            dormitoryIds,
            request.SearchValue,
            request.DateFrom,
            request.DateTo,
            request.Status,
            request.OrderBy,
            request.Direction,
            request.PageIndex,
            request.PageSize).ConfigureAwait(false);

        var result = new PaginationResponse<ShopManageDto>(data.Shops, data.TotalCount, request.PageIndex, request.PageSize);
        return Result.Success(result);
    }
}