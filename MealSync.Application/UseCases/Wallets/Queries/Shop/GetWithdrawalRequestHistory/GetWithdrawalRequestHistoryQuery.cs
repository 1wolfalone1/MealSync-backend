using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Wallets.Queries.Shop.GetWithdrawalRequestHistory;

public class GetWithdrawalRequestHistoryQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchValue { get; set; }

    public List<WithdrawalRequestStatus>? Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}