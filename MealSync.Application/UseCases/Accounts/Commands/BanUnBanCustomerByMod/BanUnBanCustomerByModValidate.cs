using FluentValidation;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Accounts.Commands.BanUnBanCustomerByMod;

public class BanUnBanCustomerByModValidate : AbstractValidator<BanUnBanCustomerByModCommand>
{
    public BanUnBanCustomerByModValidate()
    {
        RuleFor(q => q.CustomerId)
            .GreaterThan(0)
            .WithMessage("Customer Id phải lớn hơn 0");

        RuleFor(x => x.Status)
            .Must(x => x == AccountStatus.Banned || x == AccountStatus.Verify)
            .WithMessage("Chỉ có thể đổi trạng thái Verify (2) hoặc Banned (3)");
    }
}