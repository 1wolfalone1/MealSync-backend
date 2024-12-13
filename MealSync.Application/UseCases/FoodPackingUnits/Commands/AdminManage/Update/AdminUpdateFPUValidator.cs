using FluentValidation;

namespace MealSync.Application.UseCases.FoodPackingUnits.Commands.AdminManage.Update;

public class AdminUpdateFPUValidator : AbstractValidator<AdminUpdateFPUCommand>
{
    public AdminUpdateFPUValidator()
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