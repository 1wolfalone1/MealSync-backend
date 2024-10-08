using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Queries.GetByIds;

public class GetByIdsForCartValidate : AbstractValidator<GetByIdsForCartQuery>
{
    public GetByIdsForCartValidate()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("Vui lòng truyền danh sách id");
    }
}