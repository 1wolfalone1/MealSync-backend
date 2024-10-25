using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Reviews.Commands.ReviewOrderOfCustomer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class ReviewController : BaseApiController
{
    [HttpPost(Endpoints.REVIEW_ORDER)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> ReviewOrderOfCustomer(ReviewOrderOfCustomerCommand request)
    {
        return HandleResult(await Mediator.Send(request).ConfigureAwait(false));
    }
}