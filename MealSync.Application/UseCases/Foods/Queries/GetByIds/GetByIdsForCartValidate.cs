using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Queries.GetByIds;

public class GetByIdsForCartValidate : AbstractValidator<GetByIdsForCartQuery>
{
    public GetByIdsForCartValidate()
    {
        RuleFor(x => x.Foods)
            .NotEmpty()
            .WithMessage("Đồ ăn kiểm tra bắt buộc");

        RuleForEach(x => x.Foods).ChildRules(food =>
        {
            food.RuleFor(f => f.Id)
                .NotEmpty()
                .WithMessage("Id đồ ăn bắt buộc");

            food.RuleForEach(f => f.OptionGroupRadio).ChildRules(radio =>
            {
                radio.RuleFor(r => r.Id)
                    .GreaterThan(0)
                    .WithMessage("Lựa chọn nhóm id phải lớn hơn 0");

                radio.RuleFor(r => r.OptionId)
                    .GreaterThan(0)
                    .WithMessage("Lựa chọn id phải lớn hơn không");
            });

            food.RuleForEach(f => f.OptionGroupCheckbox).ChildRules(checkbox =>
            {
                checkbox.RuleFor(c => c.Id)
                    .GreaterThan(0)
                    .WithMessage("Lựa chọn nhóm id phải lớn hơn 0");

                checkbox.RuleFor(c => c.OptionIds)
                    .NotEmpty()
                    .WithMessage("Lựa chọn id bắt buộc")
                    .Must(ids => ids.All(id => id > 0))
                    .WithMessage("Lựa chọn ids phải lớn hơn 0");
            });
        });
    }
}