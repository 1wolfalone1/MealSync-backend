using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Reports.Queries.GetByOrderId;

public class GetByOrderIdQuery : IQuery<Result>
{
    public long OrderId { get; set; }
}