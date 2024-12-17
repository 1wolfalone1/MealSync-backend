using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Queries.Web.StatisticSummary;

public class StatisticSummaryQuery : IQuery<Result>
{
    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}