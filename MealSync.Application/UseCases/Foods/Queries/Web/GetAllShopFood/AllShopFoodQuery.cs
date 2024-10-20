using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Queries.Web.GetAllShopFood;

public class AllShopFoodQuery : PaginationRequest, IQuery<Result>
{
    public string? Name { get; set; }

    public int StatusMode { get; set; }

    public long? OperatingSlotId { get; set; }
}