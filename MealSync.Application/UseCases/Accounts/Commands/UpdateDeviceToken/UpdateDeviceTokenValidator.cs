using FluentValidation;

namespace MealSync.Application.UseCases.Accounts.Commands.UpdateDeviceToken;

public class UpdateDeviceTokenValidator : AbstractValidator<UpdateDeviceTokenCommand>
{
    public UpdateDeviceTokenValidator()
    {
    }
}