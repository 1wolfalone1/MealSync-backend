using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Dormitories.Models;

namespace MealSync.Application.UseCases.Dormitories.Queries.GetModDormitory;

public class GetModDormitoryHandler : IQueryHandler<GetModDormitoryQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IMapper _mapper;

    public GetModDormitoryHandler(
        ICurrentPrincipalService currentPrincipalService, IModeratorDormitoryRepository moderatorDormitoryRepository, IMapper mapper)
    {
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetModDormitoryQuery request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllIncludeDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        return Result.Success(_mapper.Map<List<DormitoryResponse>>(dormitories.Select(d => d.Dormitory).ToList()));
    }
}