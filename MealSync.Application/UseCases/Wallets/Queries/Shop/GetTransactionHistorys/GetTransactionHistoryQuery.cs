using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Wallets.Queries.Shop.GetTransactionHistorys;

public class GetTransactionHistoryQuery : PaginationRequest, IQuery<Result>
{
}