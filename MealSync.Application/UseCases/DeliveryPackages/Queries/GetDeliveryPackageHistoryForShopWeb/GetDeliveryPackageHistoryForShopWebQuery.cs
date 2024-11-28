using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetDeliveryPackageHistoryForShopWeb;

public class GetDeliveryPackageHistoryForShopWebQuery : PaginationRequest, IQuery<Result>
{
    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public string? SearchValue { get; set; }

    public DeliveryPackageHistoryFilter StatusMode { get; set; }
}

public enum DeliveryPackageHistoryFilter
{
    All = 0,
    InProcess = 1,
    Done = 2,
}