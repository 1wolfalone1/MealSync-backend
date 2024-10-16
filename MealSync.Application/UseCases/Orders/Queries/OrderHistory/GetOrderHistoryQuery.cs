using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.OrderHistory;

public class GetOrderHistoryQuery : PaginationRequest, IQuery<Result>
{
}