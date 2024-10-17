using FluentValidation;

namespace MealSync.Application.UseCases.OptionGroups.Queries.GetOptionGroupDetail;

public class GetOptionGroupDetailValidator : AbstractValidator<GetOptionGroupDetailQuery>
{
    public GetOptionGroupDetailValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id nhóm lựa chọn");
    }
}