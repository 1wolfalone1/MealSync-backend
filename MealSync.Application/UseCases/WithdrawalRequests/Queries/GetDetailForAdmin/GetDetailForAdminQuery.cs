using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.WithdrawalRequests.Queries.GetDetailForAdmin;

public class GetDetailForAdminQuery : IQuery<Result>
{
    public long WithdrawalRequestId { get; set; }
}