using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Dashboards.Queries.GetOrderChartForAdmin;
using MealSync.Application.UseCases.Dashboards.Queries.GetOverviewAdminChart;
using MealSync.Application.UseCases.Dashboards.Queries.GetRevenueAdminChart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class DashboardController : BaseApiController
{
    [HttpGet(Endpoints.ADMIN_ORDER_CHART)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> GetOrderChart([FromQuery] GetOrderChartForAdminQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.ADMIN_OVERVIEW_CHART)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> GetOverviewChart([FromQuery] GetOverviewAdminChartQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.ADMIN_REVENUE_CHART)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> GetOverviewChart([FromQuery] GetRevenueAdminChartQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }
}