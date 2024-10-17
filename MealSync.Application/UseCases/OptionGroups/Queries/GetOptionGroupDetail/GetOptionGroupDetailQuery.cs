using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.OptionGroups.Queries.GetOptionGroupDetail;

public class GetOptionGroupDetailQuery : IQuery<Result>
{
    public long Id { get; set; }
}