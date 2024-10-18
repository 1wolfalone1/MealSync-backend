using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Payments.Queries.CheckOnlinePaymentStatus;

public class CheckOnlinePaymentStatusQuery : IQuery<Result>
{
    public long Id { get; set; }
}