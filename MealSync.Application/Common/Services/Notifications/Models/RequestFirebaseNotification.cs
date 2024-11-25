namespace MealSync.Application.Common.Services.Notifications.Models;

public class RequestFirebaseNotification
{
    public long FromAccountId { get; set; }

    public long ToAccountId { get; set; }

    public string? Message { get; set; }
}