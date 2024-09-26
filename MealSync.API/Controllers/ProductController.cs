using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Commands.LoginPassword;
using MealSync.Application.UseCases.Accounts.Commands.ShopRegister;
using MealSync.Application.UseCases.Accounts.Commands.SignupCustomer;
using MealSync.Application.UseCases.Products.Commands.Create;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class ProductController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_PRODUCT)]
    public async Task<IActionResult> CreateProduct(CreateProductCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }
}