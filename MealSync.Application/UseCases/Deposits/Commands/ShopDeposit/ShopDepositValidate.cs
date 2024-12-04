using FluentValidation;

namespace MealSync.Application.UseCases.Deposits.Commands.ShopDeposit;

public class ShopDepositValidate : AbstractValidator<ShopDepositCommand>
{
    public ShopDepositValidate()
    {
        RuleFor(p => p.Amount)
            .GreaterThanOrEqualTo(50000)
            .WithMessage("Số tiền nạp phải lớn hơn hoặc bằng 50.000 VNĐ.");
    }
}