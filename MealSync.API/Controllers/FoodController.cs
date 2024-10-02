using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Foods.Commands.Create;
using MealSync.Application.UseCases.Foods.Queries.TopFood;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class FoodController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_FOOD)]
    public async Task<IActionResult> CreateFood(CreateFoodCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpGet(Endpoints.GET_TOP_FOOD)]
    public async Task<IActionResult> GetTopFood(int pageIndex, int pageSize)
    {
        return HandleResult(await Mediator.Send(new GetTopFoodQuery
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
        }).ConfigureAwait(false));
    }
}