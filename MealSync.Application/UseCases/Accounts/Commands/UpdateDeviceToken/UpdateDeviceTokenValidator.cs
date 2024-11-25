using FluentValidation;

namespace MealSync.Application.UseCases.Accounts.Commands.UpdateDeviceToken;

public class UpdateDeviceTokenValidator : AbstractValidator<UpdateDeviceTokenCommand>
{
    public UpdateDeviceTokenValidator()
    {
        RuleFor(x => x.DeviceToken)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp device token");
    }
}