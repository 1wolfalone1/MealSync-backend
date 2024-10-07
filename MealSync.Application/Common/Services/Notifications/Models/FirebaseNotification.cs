using Newtonsoft.Json;

namespace MealSync.Application.Common.Services.Notifications.Models;

public class FirebaseNotification
{
    [JsonProperty("token")]
    public string Token { get; set; } = default!;

    [JsonProperty("notification")]
    public FirebaseNotificationContent Notification { get; set; } = default!;

    [JsonProperty("data")]
    public object? Data { get; set; } = default!;
}

public class FirebaseNotificationContent
{
    [JsonProperty("title")]
    public string Title { get; set; } = default!;

    [JsonProperty("body")]
    public string Body { get; set; } = default!;

    [JsonProperty("body")]
    public string ImageUrl { get; set; } = default!;
}