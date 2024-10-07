using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Favourites.Models;

namespace MealSync.Application.UseCases.Favourites.Queries.FavouriteShop;

public class GetFavouriteShopHandler : IQueryHandler<GetFavouriteShopQuery, Result>
{
    private readonly IFavouriteRepository _favouriteRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetFavouriteShopHandler(IFavouriteRepository favouriteRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _favouriteRepository = favouriteRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetFavouriteShopQuery request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;

        var favourites = await _favouriteRepository.GetAllByAccountId(accountId, request.PageIndex, request.PageSize)
            .ConfigureAwait(false);

        var result = new PaginationResponse<ShopFavouriteResponse>(
            _mapper.Map<List<ShopFavouriteResponse>>(favourites.Favourites.Select(f => f.Shop).ToList()),
            favourites.TotalCount,
            request.PageIndex,
            request.PageSize);

        return Result.Success(result);
    }
}