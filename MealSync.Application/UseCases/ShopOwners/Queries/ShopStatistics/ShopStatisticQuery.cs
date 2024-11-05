using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Queries.ShopStatistics;

public class ShopStatisticQuery : IQuery<Result>
{
    private static readonly DateTimeOffset CurrentTimeOffset = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));

    public DateTime StartDate { get; set; } = new DateTime(CurrentTimeOffset.Year, CurrentTimeOffset.Month, 1);

    public DateTime EndDate { get; set; } = CurrentTimeOffset.DateTime;

    public int NumberTopProduct { get; set; } = 5;
}