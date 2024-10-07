using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Services.Notifications;

public interface INotificationService
{
    Task NotifyAsync(Notification notification);
}