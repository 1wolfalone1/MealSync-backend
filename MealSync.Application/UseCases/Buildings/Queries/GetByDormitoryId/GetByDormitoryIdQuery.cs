using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Buildings.Queries.GetByDormitoryId;

public class GetByDormitoryIdQuery : IQuery<Result>
{
    public int DormitoryId { get; set; }

    public string? Query { get; set; }
}