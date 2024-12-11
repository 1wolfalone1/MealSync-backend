using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Accounts.Queries.AdminManage.GetDetailAccount;

public class GetDetailAccountAdminQuery : IQuery<Result>
{
    public long AccountId { get; set; }
}