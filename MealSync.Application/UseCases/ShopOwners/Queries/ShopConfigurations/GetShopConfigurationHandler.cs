using System.Linq.Expressions;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.UseCases.ShopOwners.Queries.ShopConfigurations;

public class GetShopConfigurationHandler : IQueryHandler<GetShopConfigurationQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopRepository _shopRepository;
    private readonly IMapper _mapper;

    public GetShopConfigurationHandler(ICurrentPrincipalService currentPrincipalService, IShopRepository shopRepository, IMapper mapper)
    {
        _currentPrincipalService = currentPrincipalService;
        _shopRepository = shopRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetShopConfigurationQuery request, CancellationToken cancellationToken)
    {
        var shopConfigInfo = this._shopRepository.GetShopConfiguration(this._currentPrincipalService.CurrentPrincipalId.Value);
        var response = this._mapper.Map<ShopConfigurationResponse>(shopConfigInfo);

        return Result.Success(response);
    }
}