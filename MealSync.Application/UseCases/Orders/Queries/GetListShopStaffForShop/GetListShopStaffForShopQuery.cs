using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.GetListShopStaffForShop;

public class GetListShopStaffForShopQuery : ICommand<Result>
{
    public string? SearchText { get; set; }

    public int OrderByMode { get; set; }
}