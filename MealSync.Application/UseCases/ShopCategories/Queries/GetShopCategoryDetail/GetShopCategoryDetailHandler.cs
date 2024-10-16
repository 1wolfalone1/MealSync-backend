using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopCategories.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Application.UseCases.ShopCategories.Queries.GetShopCategoryDetail;

public class GetShopCategoryDetailHandler : IQueryHandler<GetShopCategoryDetailQuery, Result>
{
    private readonly IShopCategoryRepository _shopCategoryRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetShopCategoryDetailHandler(IShopCategoryRepository shopCategoryRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService)
    {
        _shopCategoryRepository = shopCategoryRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetShopCategoryDetailQuery request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var shopCategory = _shopCategoryRepository.Get(sc => sc.Id == request.Id)
            .Include(sc => sc.Foods.Where(f => f.Status != FoodStatus.Delete))
            .SingleOrDefault();
        var response = _mapper.Map<ShopCategoryDetailResponse>(shopCategory);
        return Result.Success(response);
    }

    private void Validate(GetShopCategoryDetailQuery request)
    {
        if (_shopCategoryRepository.GetByIdAndShopId(request.Id, _currentPrincipalService.CurrentPrincipalId.Value) == default)
            throw new InvalidBusinessException(MessageCode.E_SHOP_CATEGORY_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);
    }
}
