using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.FoodPackingUnits.Queries.GetFPUForAdmin;

public class GetFPUForAdminQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchText { get; set; }

    public FPUTypeMode Type { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}

public enum FPUTypeMode
{
    All = 0,
    System = 1,
    Shop = 2,
}