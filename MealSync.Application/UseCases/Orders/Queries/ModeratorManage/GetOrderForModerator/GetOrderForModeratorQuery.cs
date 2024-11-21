using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Queries.ModeratorManage.GetOrderForModerator;

public class GetOrderForModeratorQuery : PaginationRequest, IQuery<Result>
{
    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public OrderStatus[] Status { get; set; }

    public string? SearchValue { get; set; }

    public long[] DormitoryIds { get; set; }
}