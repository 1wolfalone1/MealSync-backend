using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Wallets.Queries.Shop.GetWithdrawalRequestDetail;

public class GetWithdrawalRequestDetailQuery : IQuery<Result>
{
    public long Id { get; set; }
}