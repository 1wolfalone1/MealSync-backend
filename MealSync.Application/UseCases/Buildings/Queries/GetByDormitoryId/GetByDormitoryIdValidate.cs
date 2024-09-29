using FluentValidation;

namespace MealSync.Application.UseCases.Buildings.Queries.GetByDormitoryId;

public class GetByDormitoryIdValidate : AbstractValidator<GetByDormitoryIdQuery>
{
    public GetByDormitoryIdValidate()
    {
        RuleFor(e => e.DormitoryId)
            .NotNull()
            .Must(id => id > 0)
            .WithMessage("DormitoryId phải lớn hơn 0");
    }
}