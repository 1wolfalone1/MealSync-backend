using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.GetEvidenceDeliveryFail;

public class GetEvidenceDeliveryFailQuery : IQuery<Result>
{
    public long OrderId { get; set; }
}