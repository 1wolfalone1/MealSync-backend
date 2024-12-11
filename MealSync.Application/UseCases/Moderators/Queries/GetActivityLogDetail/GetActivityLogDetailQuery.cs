using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Moderators.Queries.GetActivityLogDetail;

public class GetActivityLogDetailQuery : IQuery<Result>
{
    public long Id { get; set; }
}