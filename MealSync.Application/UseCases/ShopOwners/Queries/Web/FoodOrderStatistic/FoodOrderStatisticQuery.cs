using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Queries.Web.FoodOrderStatistic;

public class FoodOrderStatisticQuery : IQuery<Result>
{
    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}