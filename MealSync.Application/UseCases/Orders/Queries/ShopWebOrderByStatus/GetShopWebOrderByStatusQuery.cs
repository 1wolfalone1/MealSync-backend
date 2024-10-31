using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Queries.ShopWebOrderByStatus;

public class GetShopWebOrderByStatusQuery : PaginationRequest, IQuery<Result>
{
    public string? Id { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTimeOffset? IntendedReceiveDate { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public OrderStatus[] Status { get; set; }
}