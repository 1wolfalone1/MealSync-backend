using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.FoodPackingUnits.Commands.AdminManage.Create;

public class AdminCreateFoodPackingUnitCommand : ICommand<Result>
{
    public string Name { get; set; }

    public double Weight { get; set; }
}