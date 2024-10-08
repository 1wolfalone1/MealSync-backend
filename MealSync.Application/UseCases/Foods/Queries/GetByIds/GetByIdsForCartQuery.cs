using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Queries.GetByIds;

public class GetByIdsForCartQuery : IQuery<Result>
{
    public List<long> Ids { get; set; }
}