using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using MealSync.Application.Common.Services;

namespace MealSync.Infrastructure.Services;

public class FirebaseNotificationService : BaseService, IFirebaseNotificationService
{
    private readonly IConfiguration _configuration;

    public FirebaseNotificationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void CreateFirebaseAuth()
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.GetApplicationDefault(),
                ProjectId = _configuration["PROJECT_ID"],
            });
        }
    }

    public async Task<bool> SendNotification(string deviceToken, string title, string body, string imageUrl = null)
    {
        CreateFirebaseAuth();
        var message = new Message()
        {
            Notification = new Notification()
            {
                Title = title,
                Body = body,
                ImageUrl = imageUrl == string.Empty
                    ? string.Empty
                    : imageUrl,
            },
            Token = deviceToken,
        };

        try
        {
            // Send the notification
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            Console.WriteLine("Successfully sent message: " + response);
            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            Console.WriteLine("Error sending message: " + ex.Message);
            return false;
        }
    }
}