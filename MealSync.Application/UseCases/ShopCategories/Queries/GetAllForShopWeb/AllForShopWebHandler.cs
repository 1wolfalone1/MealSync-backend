using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopCategories.Models;

namespace MealSync.Application.UseCases.ShopCategories.Queries.GetAllForShopWeb;

public class AllForShopWebHandler : IQueryHandler<AllForShopWebQuery, Result>
{
    private readonly IShopCategoryRepository _shopCategoryRepository;
    private readonly IMapper _mapper;

    public AllForShopWebHandler(IShopCategoryRepository shopCategoryRepository, IMapper mapper)
    {
        _shopCategoryRepository = shopCategoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(AllForShopWebQuery request, CancellationToken cancellationToken)
    {
        var shopCate = await _shopCategoryRepository.GetAllShopCategoriesAsync(request.PageIndex, request.PageSize, request.Name).ConfigureAwait(false);
        var cates = _mapper.Map<List<ShopCategoryForShopWebResponse>>(shopCate.ShopCategories);
        return Result.Success(new PaginationResponse<ShopCategoryForShopWebResponse>(cates, shopCate.TotalCount, request.PageIndex, request.PageSize));
    }
}