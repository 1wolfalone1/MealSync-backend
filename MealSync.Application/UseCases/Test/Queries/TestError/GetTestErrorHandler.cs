using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Test.Queries.TestError;

public class GetTestErrorHandler : IQueryHandler<GetTestErrorQuery, Result>
{
    private readonly IActivityLogRepository _activityLogRepository;

    public GetTestErrorHandler(IActivityLogRepository activityLogRepository)
    {
        _activityLogRepository = activityLogRepository;
    }

    public async Task<Result<Result>> Handle(GetTestErrorQuery request, CancellationToken cancellationToken)
    {
        var list = this._activityLogRepository.Get().ToList();
        return Result.Success(list);
    }
}