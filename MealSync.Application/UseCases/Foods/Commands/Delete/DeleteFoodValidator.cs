using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Commands.Delete;

public class DeleteFoodValidator : AbstractValidator<DeleteFoodCommand>
{
    public DeleteFoodValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của món ăn cần xóa");
    }
}