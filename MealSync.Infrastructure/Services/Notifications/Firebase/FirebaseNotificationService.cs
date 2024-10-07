using System.Diagnostics;
using CorePush.Firebase;
using FirebaseAdmin.Messaging;
using MealSync.Application.Common.Repositories;
using Microsoft.Extensions.Configuration;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Services.Notifications.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MealSync.Infrastructure.Services;

public class FirebaseNotificationService : BaseService, IMobileNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FirebaseNotificationService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public FirebaseNotificationService(IConfiguration configuration, ILogger<FirebaseNotificationService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task NotifyAsync(Domain.Entities.Notification notification)
    {
        // Get device token
        using var scope = _serviceScopeFactory.CreateScope();
        var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
        var account = accountRepository.GetById(notification.AccountId);
        if (account != default)
        {
            var message = new FirebaseNotification()
            {
                Notification = new FirebaseNotificationContent()
                {
                    Title = notification.Title,
                    Body = notification.Content,
                    ImageUrl = notification.ImageUrl ?? string.Empty,
                },
                Token = account.DeviceToken,
                Data = new
                {
                    EntityType = notification.EntityType.ToString(),
                    ReferenceId = notification.ReferenceId,
                },
            };

            Debug.WriteLine($"Send notification to device with token: {account.DeviceToken}");
            try
            {
                // Send the notification
                var firebaseSetting = new FirebaseSettings(_configuration["FIREBASE_PROJECT_ID"], _configuration["FIREBASE_PRIVATE_KEY"], _configuration["FIREBASE_CLIENT_EMAIL"], _configuration["FIREBASE_TOKEN_URI"]);
                var fcmSender = new FirebaseSender(firebaseSetting, new HttpClient());
                var response = await fcmSender.SendAsync(message).ConfigureAwait(false);
                _logger.LogInformation("Successfully sent message: " + response);
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError(ex, "Error sending message: " + ex.Message);
            }
        }
    }
}