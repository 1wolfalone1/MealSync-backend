using FluentValidation;

namespace MealSync.Application.UseCases.FoodPackingUnits.Commands.Update.ShopUpdateFPU;

public class ShopUpdateFPUValidator : AbstractValidator<ShopUpdateFPUCommand>
{
    public ShopUpdateFPUValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp tên");

        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .WithMessage("Vui lòng cung cấp cân nặng đã bao gốm thức ăn > 0");
    }
}