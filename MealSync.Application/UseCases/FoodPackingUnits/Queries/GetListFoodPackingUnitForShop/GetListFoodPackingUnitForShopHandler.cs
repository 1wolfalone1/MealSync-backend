using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.FoodPackingUnits.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.FoodPackingUnits.Queries.GetListFoodPackingUnitForShop;

public class GetListFoodPackingUnitForShopHandler : IQueryHandler<GetListFoodPackingUnitForShopQuery, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFoodPackingUnitRepository _foodPackingUnitRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<GetListFoodPackingUnitForShopHandler> _logger;
    private readonly IMapper _mapper;

    public GetListFoodPackingUnitForShopHandler(IUnitOfWork unitOfWork, IFoodPackingUnitRepository foodPackingUnitRepository, ICurrentPrincipalService currentPrincipalService, ILogger<GetListFoodPackingUnitForShopHandler> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _foodPackingUnitRepository = foodPackingUnitRepository;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetListFoodPackingUnitForShopQuery request, CancellationToken cancellationToken)
    {
        var foodPackingPaging = _foodPackingUnitRepository.GetShopFoodPackingUnitsPaging(_currentPrincipalService.CurrentPrincipalId.Value, request.SearchText,request.PageIndex, request.PageSize);
        var response = _mapper.Map<List<FoodPackingUnitResponse>>(foodPackingPaging.FoodPackingUnits);

        return Result.Success(new PaginationResponse<FoodPackingUnitResponse>(response, foodPackingPaging.TotalCount, request.PageIndex, request.PageSize));
    }

}