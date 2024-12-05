using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Shops.Models;

namespace MealSync.Application.UseCases.Shops.Queries.GetShopInCart;

public class GetShopInCartHandler : IQueryHandler<GetShopInCartQuery, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly IMapper _mapper;

    public GetShopInCartHandler(IShopRepository shopRepository, IMapper mapper)
    {
        _shopRepository = shopRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetShopInCartQuery request, CancellationToken cancellationToken)
    {
        var shops = await _shopRepository.GetShopByIds(request.Ids).ConfigureAwait(false);
        return Result.Success(_mapper.Map<List<ShopSummaryResponse>>(shops));
    }
}