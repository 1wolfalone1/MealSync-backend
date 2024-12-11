using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.OrderDetailForAdmin;

public class OrderDetailForAdminQuery : IQuery<Result>
{
    public long Id { get; set; }
}