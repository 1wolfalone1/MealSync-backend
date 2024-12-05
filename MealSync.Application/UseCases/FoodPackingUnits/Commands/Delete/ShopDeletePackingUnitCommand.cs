using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.FoodPackingUnits.Commands.Delete;

public class ShopDeletePackingUnitCommand : ICommand<Result>
{
    public long Id { get; set; }

    public bool IsConfirm { get; set; }
}