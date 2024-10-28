using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetListShopStaffForShop;

public class GetListShopStaffForShopQuery : ICommand<Result>
{
    public string? SearchText { get; set; }

    public int OrderByMode { get; set; }

    public DateTime IntendedReceiveDate { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }
}