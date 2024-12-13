using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.FoodPackingUnits.Models;

namespace MealSync.Application.UseCases.FoodPackingUnits.Queries.GetFPUForAdmin;

public class GetFPUForAdminHandler : IQueryHandler<GetFPUForAdminQuery, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFoodPackingUnitRepository _foodPackingUnitRepository;
    private readonly IMapper _mapper;

    public GetFPUForAdminHandler(IUnitOfWork unitOfWork, IFoodPackingUnitRepository foodPackingUnitRepository, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _foodPackingUnitRepository = foodPackingUnitRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetFPUForAdminQuery request, CancellationToken cancellationToken)
    {
        var foodPaging = _foodPackingUnitRepository.GetShopFoodPackingUnitAdminPaging(request.SearchText, (int)request.Type, request.DateFrom, request.DateTo, request.PageIndex, request.PageSize);
        var response = _mapper.Map<List<FoodPackingUnitAdminResponse>>(foodPaging.FoodPackingUnits);

        return Result.Success(new PaginationResponse<FoodPackingUnitAdminResponse>(response, foodPaging.TotalCount, request.PageIndex, request.PageSize));
    }
}