using FluentValidation;

namespace MealSync.Application.UseCases.Accounts.Commands.CheckValidTokenJwt;

public class CheckValidTokenJwtValidator : AbstractValidator<CheckValidTokenJwtCommand>
{
    public CheckValidTokenJwtValidator()
    {
        When(x => x.Role.HasValue, () =>
        {
            RuleFor(x => x.Role)
                .IsInEnum()
                .WithMessage("Vui lòng cung cấp role đúng từ 1 - 5");
        });
    }
}