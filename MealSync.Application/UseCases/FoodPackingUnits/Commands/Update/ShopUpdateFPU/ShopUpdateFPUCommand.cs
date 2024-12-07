using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.FoodPackingUnits.Commands.Update.ShopUpdateFPU;

public class ShopUpdateFPUCommand : ICommand<Result>
{
    public long Id { get; set; }

    public string Name { get; set; }

    public double Weight { get; set; }
}