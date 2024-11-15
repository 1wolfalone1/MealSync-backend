using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Queries.GetFoodReorder;

public class GetFoodReorderQuery : IQuery<Result>
{
    public long OrderId { get; set; }

    public long OperatingSlotId { get; set; }

    public bool IsOrderForNextDay { get; set; }

    public long BuildingOrderId { get; set; }
}