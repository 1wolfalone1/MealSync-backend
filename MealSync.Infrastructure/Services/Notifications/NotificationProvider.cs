using MealSync.Application.Common.Services.Notifications;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Infrastructure.Services.Notifications;

public class NotificationProvider : INotificationProvider
{
    private readonly Dictionary<NotificationTypes, IList<INotificationService>> _observers = new();

    public void Attach(NotificationTypes type, INotificationService notificationService)
    {
        if (!_observers.ContainsKey(type))
        {
            _observers[type] = new List<INotificationService>();
        }

        var services = _observers[type];
        if (services.All(service => service.GetType() != notificationService.GetType()))
        {
            services.Add(notificationService);
        }
    }

    public void Attach(ICollection<NotificationTypes> types, INotificationService notificationService)
    {
        foreach (var type in types)
        {
            Attach(type, notificationService);
        }
    }

    public void Attach(NotificationTypes type, ICollection<INotificationService> notificationServices)
    {
        foreach (var service in notificationServices)
        {
            Attach(type, service);
        }
    }

    public void Detach(NotificationTypes type, INotificationService notificationService)
    {
        if (_observers.ContainsKey(type))
        {
            _observers[type].Remove(notificationService);
        }
    }

    public async Task NotifyAsync(Notification notification)
    {
        if (_observers.TryGetValue(notification.Type, out var services))
        {
            foreach (var service in services)
            {
                await service.NotifyAsync(notification);
            }
        }
    }
}