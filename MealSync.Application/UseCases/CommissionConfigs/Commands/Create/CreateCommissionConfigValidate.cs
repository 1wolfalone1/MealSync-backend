using FluentValidation;

namespace MealSync.Application.UseCases.CommissionConfigs.Commands.Create;

public class CreateCommissionConfigValidate : AbstractValidator<CreateCommissionConfigCommand>
{
    public CreateCommissionConfigValidate()
    {
        RuleFor(q => q.CommissionRate)
            .GreaterThan(0)
            .WithMessage("Tỷ lệ hoa hồng phải lớn hơn 0");
    }
}