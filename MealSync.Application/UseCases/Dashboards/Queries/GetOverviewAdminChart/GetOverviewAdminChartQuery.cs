using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Dashboards.Queries.GetOverviewAdminChart;

public class GetOverviewAdminChartQuery : IQuery<Result>
{
    public DateTime DateFrom { get; set; }

    public DateTime DateTo { get; set; }
}