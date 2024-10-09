using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.OptionGroups.Queries.GetAllOptionGroupOfShop;

public class GetAllShopOptionGroupQuery : PaginationRequest, IQuery<Result>
{
}