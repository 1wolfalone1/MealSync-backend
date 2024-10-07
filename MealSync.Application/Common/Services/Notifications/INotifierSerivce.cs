using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Services.Notifications;

public interface INotifierSerivce
{
    Task NotifyAsync(Notification notification);
}