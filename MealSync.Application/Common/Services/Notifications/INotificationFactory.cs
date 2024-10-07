using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Services.Notifications;

public interface INotificationFactory
{
    Notification CreateOrderRejectedNotification(Order order, Shop shop);

    Notification CreateOrderCancelNotification(Order order, Shop shop);
}