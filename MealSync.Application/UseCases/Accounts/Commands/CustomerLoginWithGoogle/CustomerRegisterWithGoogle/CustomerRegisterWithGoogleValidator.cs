using FluentValidation;

namespace MealSync.Application.UseCases.Accounts.Commands.CustomerLoginWithGoogle.CustomerRegisterWithGoogle;

public class CustomerRegisterWithGoogleValidator : AbstractValidator<CustomerRegisterWithGoogleCommand>
{
    public CustomerRegisterWithGoogleValidator()
    {
        RuleFor(x => x.CodeConfirm)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp code để xác nhận");

        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id địa chỉ tòa ký túc");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp số điện thoại")
            .Matches(RegularPatternConstant.VN_PHONE_NUMBER_PATTERN)
            .WithMessage("Vui lòng cung cấp số điện thoại đúng");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp email");

        RuleFor(x => x.FUserId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp Firebase user id");
    }
}