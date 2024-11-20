using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Notifications.Commands.UpdateReadedNotification;

public class UpdateReadedNotificationCommand : ICommand<Result>
{
    public long[] Ids { get; set; }
}