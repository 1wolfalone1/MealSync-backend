using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopCategories.Commands.LinkFoods;

public class LinkFoodCommand : ICommand<Result>
{
    public long FoodId { get; set; }

    public long ShopCategoryId { get; set; }
}