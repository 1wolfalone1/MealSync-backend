using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Services.Notifications;

public interface INotificationProvider
{
    void Attach(NotificationTypes type, INotificationService notificationService);

    void Attach(ICollection<NotificationTypes> types, INotificationService notificationService);

    void Attach(NotificationTypes type, ICollection<INotificationService> notificationServices);

    void Detach(NotificationTypes type, INotificationService notificationService);

    Task NotifyAsync(Notification notification);
}