using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.OptionGroups.Queries.GetAllOptionGroupOfShop;

public class GetAllShopOptionGroupQuery : PaginationRequest, IQuery<Result>
{
    public string? Title { get; set; }

    public int Status { get; set; }
}