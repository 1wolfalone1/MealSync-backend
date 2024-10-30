using FluentValidation;

namespace MealSync.Application.UseCases.Wallets.Commands.Shop.SendCodeWithdrawalRequest;

public class SendCodeWithdrawalRequestValidate : AbstractValidator<SendCodeWithdrawalRequestCommand>
{
    public SendCodeWithdrawalRequestValidate()
    {
        RuleFor(p => p.Amount)
            .GreaterThan(0)
            .WithMessage("Số tiền rút phải lớn hơn 0.");

        RuleFor(p => p.BankCode)
            .NotEmpty()
            .WithMessage("Mã ngân hàng không được để trống.");

        RuleFor(p => p.BankShortName)
            .NotEmpty()
            .WithMessage("Tên của ngân hàng không được để trống.");

        RuleFor(p => p.BankAccountNumber)
            .NotEmpty()
            .WithMessage("Số tài khoản ngân hàng không được để trống.")
            .Matches(@"^\d{6,20}$")
            .WithMessage("Số tài khoản phải chứa từ 6 đến 20 chữ số.");
    }
}