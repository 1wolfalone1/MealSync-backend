using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Orders.Queries.Dashboards.GetOrderChartForAdmin;
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
}