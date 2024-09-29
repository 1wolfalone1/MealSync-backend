using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Dormitories.Models;

namespace MealSync.Application.UseCases.Dormitories.Queries.GetAll;

public class GetAllDormitoryHandler : IQueryHandler<GetAllDormitoryQuery, Result>
{

    private readonly IDormitoryRepository _dormitoryRepository;
    private readonly IMapper _mapper;

    public GetAllDormitoryHandler(IDormitoryRepository dormitoryRepository, IMapper mapper)
    {
        _dormitoryRepository = dormitoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetAllDormitoryQuery request, CancellationToken cancellationToken)
    {
        var dormitories = _dormitoryRepository.GetAll();
        return Result.Success(_mapper.Map<List<DormitoryResponse>>(dormitories));
    }
}