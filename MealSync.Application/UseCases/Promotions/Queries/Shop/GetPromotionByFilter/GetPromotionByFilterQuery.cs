using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Promotions.Queries.Shop.GetPromotionByFilter;

public class GetPromotionByFilterQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchValue { get; set; }

    public PromotionStatus? Status { get; set; }

    public PromotionApplyTypes? ApplyType { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}