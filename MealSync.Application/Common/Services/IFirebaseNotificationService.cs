namespace MealSync.Application.Common.Services;

public interface IFirebaseNotificationService
{
    Task<bool> SendNotification(string deviceToken, string title, string body, string imageUrl = null);
}