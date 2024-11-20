using FluentValidation;

namespace MealSync.Application.UseCases.Notifications.Commands.UpdateReadedNotification;

public class UpdateReadedNotificationValidator : AbstractValidator<UpdateReadedNotificationCommand>
{
    public UpdateReadedNotificationValidator()
    {
        RuleFor(x => x.Ids)
            .Must(x => x.Count() > 0)
            .WithMessage("Vui lòng cung cấp ít nhất 1 id thông báo");
    }
}