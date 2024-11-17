using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Dashboards.Queries.GetOrderChartForAdmin;

public class GetOrderChartForAdminQuery : IQuery<Result>
{
    public DateTime DateFrom { get; set; }

    public DateTime DateTo { get; set; }
}