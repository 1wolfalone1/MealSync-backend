using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopCategories.Commands.Create;

public class CreateShopCategoryCommand : ICommand<Result>
{
    public string Name { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }
}