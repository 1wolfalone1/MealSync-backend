using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Accounts.Queries.ModeratorManage.GetDetailAccount;

public class GetDetailAccountQuery : IQuery<Result>
{
    public long AccountId { get; set; }
}