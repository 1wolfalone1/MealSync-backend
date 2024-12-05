using FluentValidation;

namespace MealSync.Application.UseCases.FoodPackingUnits.Commands.Create.ShopCreate;

public class ShopCreateFoodPackingUnitValidator : AbstractValidator<ShopCreateFoodPackingUnitCommand>
{
    public ShopCreateFoodPackingUnitValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp tên");

        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .WithMessage("Vui lòng cung cấp cân nặng đã bao gốm thức ăn > 0");
    }
}