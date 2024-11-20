using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Confluent.Kafka;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Services.Notifications.Models;
using MealSync.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MealSync.Infrastructure.Services.Notifications.Kafka;

public class KafkaPushNotificationService : BaseService, IWebNotificationService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaPushNotificationService> _logger;

    public KafkaPushNotificationService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<KafkaPushNotificationService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task NotifyAsync(Notification notification)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var config = new ProducerConfig
        {
            BootstrapServers = _configuration["KAFKA_BOOSTRAP_SERVER"]
        };

        var message = new FirebaseNotification()
        {
            Notification = new FirebaseNotificationContent()
            {
                Title = notification.Title,
                Body = notification.Content,
                ImageUrl = notification.ImageUrl ?? string.Empty,
            },
            // Token = account.DeviceToken,
            Data = new
            {
                EntityType = notification.EntityType.ToString(),
                ReferenceId = notification.ReferenceId,
            },
        };
        var messageValue = JsonSerializer.Serialize(message);
        using (var producer = new ProducerBuilder<string, string>(config).Build())
        {
            try
            {
                // Send the message asynchronously
                var result = await producer.ProduceAsync(_configuration["KAFKA_NOTI_TOPIC"], new Message<string, string>
                {
                    Key = "Notification",
                    Value = messageValue,
                });

                // Log the result
                _logger.LogInformation($"Message sent to partition {result.Partition} with offset {result.Offset}");
            }
            catch (ProduceException<string, string> e)
            {
                _logger.LogError($"Error: {e.Error.Reason}");
                Console.WriteLine($"Error: {e.Error.Reason}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}