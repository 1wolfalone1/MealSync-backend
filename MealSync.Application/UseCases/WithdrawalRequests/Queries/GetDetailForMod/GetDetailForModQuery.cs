using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.WithdrawalRequests.Queries.GetDetailForMod;

public class GetDetailForModQuery : IQuery<Result>
{
    public long WithdrawalRequestId { get; set; }
}