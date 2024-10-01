using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Foods.Commands.Create;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class FoodController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_FOOD)]
    public async Task<IActionResult> CreateFood(CreateFoodCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }
}