using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Commands.ShopUpdateFoodStatus;

public class ShopUpdateFoodStatusCommand : ICommand<Result>
{
    public long Id { get; set; }

    public FoodStatus Status { get; set; }

    public bool? IsSoldOut { get; set; }
}