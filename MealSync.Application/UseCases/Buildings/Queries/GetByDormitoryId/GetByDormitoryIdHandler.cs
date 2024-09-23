using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Buildings.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Buildings.Queries.GetByDormitoryId;

public class GetByDormitoryIdHandler : IQueryHandler<GetByDormitoryIdQuery, Result>
{
    private readonly IDormitoryRepository _dormitoryRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetByDormitoryIdHandler(IBuildingRepository buildingRepository, IMapper mapper, IDormitoryRepository dormitoryRepository)
    {
        _buildingRepository = buildingRepository;
        _mapper = mapper;
        _dormitoryRepository = dormitoryRepository;
    }

    public async Task<Result<Result>> Handle(GetByDormitoryIdQuery request, CancellationToken cancellationToken)
    {
        var existedDormitory = _dormitoryRepository.CheckExistedById(request.DormitoryId);
        if (!existedDormitory)
        {
            throw new InvalidBusinessException(MessageCode.E_DORMITORY_NOT_FOUND.GetDescription());
        }
        else
        {
            var buildings = _buildingRepository.GetByDormitoryIdAndName(
                request.DormitoryId,
                string.IsNullOrEmpty(request.Query) ? string.Empty : request.Query
            );
            return Result.Success(_mapper.Map<List<BuildingResponse>>(buildings));
        }
    }
}