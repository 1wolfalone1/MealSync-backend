using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Dashboards.Queries.GetOrderChartAdminWeb;

public class GetOrderChartAdminWebQuery : IQuery<Result>
{
    public DateTime DateFrom { get; set; }

    public DateTime DateTo { get; set; }
}