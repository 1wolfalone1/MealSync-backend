using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.FoodPackingUnits.Commands.Create.ShopCreate;

public class ShopCreateFoodPackingUnitCommand : ICommand<Result>
{
    public string Name { get; set; }

    public double Weight { get; set; }
}