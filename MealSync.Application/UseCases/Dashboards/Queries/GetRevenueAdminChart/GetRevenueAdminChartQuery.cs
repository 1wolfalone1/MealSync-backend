using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Dashboards.Queries.GetRevenueAdminChart;

public class GetRevenueAdminChartQuery : IQuery<Result>
{
    public DateTime DateOfYear { get; set; } = DateTime.Now;
}