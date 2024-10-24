using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Services.Notifications;

public interface INotifierService
{
    Task NotifyAsync(Notification notification);
}