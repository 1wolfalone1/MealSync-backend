using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Moderators.Models;

namespace MealSync.Application.UseCases.Moderators.Queries.GetActivityLog;

public class GetActivityLogHandler : IQueryHandler<GetActivityLogQuery, Result>
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IMapper _mapper;

    public GetActivityLogHandler(IActivityLogRepository activityLogRepository, IMapper mapper)
    {
        _activityLogRepository = activityLogRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetActivityLogQuery request, CancellationToken cancellationToken)
    {
        var activityLogPaging = _activityLogRepository.GetActivityLogPaging(request.SearchValue, request.TargetType, request.DateFrom, request.DateTo, request.PageIndex, request.PageSize);
        var responseList = _mapper.Map<List<ModeratorActivityLogResponse>>(activityLogPaging.Result);
        return Result.Success(new PaginationResponse<ModeratorActivityLogResponse>(responseList, activityLogPaging.TotalCount, request.PageIndex, request.PageSize));
    }
}