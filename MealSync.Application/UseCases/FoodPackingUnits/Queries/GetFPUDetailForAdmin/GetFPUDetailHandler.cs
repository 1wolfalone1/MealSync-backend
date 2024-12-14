using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.FoodPackingUnits.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.FoodPackingUnits.Queries.GetFPUDetailForAdmin;

public class GetFPUDetailHandler : IQueryHandler<GetFPUDetailQuery, Result>
{
    private readonly IFoodPackingUnitRepository _foodPackingUnitRepository;
    private readonly IMapper _mapper;

    public GetFPUDetailHandler(IFoodPackingUnitRepository foodPackingUnitRepository, IMapper mapper)
    {
        _foodPackingUnitRepository = foodPackingUnitRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetFPUDetailQuery request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var fpu = _foodPackingUnitRepository.GetById(request.Id);
        return Result.Success(_mapper.Map<FoodPackingUnitAdminResponse>(fpu));
    }

    private void Validate(GetFPUDetailQuery request)
    {
        if (_foodPackingUnitRepository.Get(x => x.Type == FoodPackingUnitType.System && x.Id == request.Id).FirstOrDefault() == null)
        {
            throw new InvalidBusinessException(MessageCode.E_FOOD_PACKING_UNIT_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);
        }
    }
}