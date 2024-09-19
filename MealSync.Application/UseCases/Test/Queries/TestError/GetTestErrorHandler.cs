using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Test.Queries.TestError;

public class GetTestErrorHandler : IQueryHandler<GetTestErrorQuery, Result>
{
    public async Task<Result<Result>> Handle(GetTestErrorQuery request, CancellationToken cancellationToken)
    {
        throw new InvalidBusinessException("TestCode", HttpStatusCode.Conflict);
    }
}