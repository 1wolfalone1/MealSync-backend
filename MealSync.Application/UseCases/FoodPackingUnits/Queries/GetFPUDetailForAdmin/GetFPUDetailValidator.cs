using FluentValidation;

namespace MealSync.Application.UseCases.FoodPackingUnits.Queries.GetFPUDetailForAdmin;

public class GetFPUDetailValidator : AbstractValidator<GetFPUDetailQuery>
{
    public GetFPUDetailValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id");
    }
}