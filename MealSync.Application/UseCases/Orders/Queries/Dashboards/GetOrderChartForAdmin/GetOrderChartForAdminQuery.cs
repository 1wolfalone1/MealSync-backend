using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.Dashboards.GetOrderChartForAdmin;

public class GetOrderChartForAdminQuery : IQuery<Result>
{
    public DateTime DateFrom { get; set; }

    public DateTime DateTo { get; set; }
}