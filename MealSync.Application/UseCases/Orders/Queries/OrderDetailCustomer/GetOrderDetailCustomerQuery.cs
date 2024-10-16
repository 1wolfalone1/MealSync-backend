using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.OrderDetailCustomer;

public class GetOrderDetailCustomerQuery : IQuery<Result>
{
    public long Id { get; set; }
}