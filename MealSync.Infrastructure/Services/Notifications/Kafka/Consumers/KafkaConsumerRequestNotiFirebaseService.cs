using Confluent.Kafka;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Services.Notifications.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MealSync.Infrastructure.Services.Notifications.Kafka.Consumers;

public class KafkaConsumerRequestNotiFirebaseService : BackgroundService
{
    private readonly ConsumerConfig _consumerConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMobileNotificationService _mobileNotificationService;
    private readonly ILogger<KafkaConsumerRequestNotiFirebaseService> _logger;

    public KafkaConsumerRequestNotiFirebaseService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<KafkaConsumerRequestNotiFirebaseService> logger, IServiceScopeFactory serviceScopeFactory, IMobileNotificationService mobileNotificationService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _mobileNotificationService = mobileNotificationService;

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["KAFKA_BOOSTRAP_SERVER"],
            GroupId = "notification-processor-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // Ensures non-blocking start

        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe("request-notification");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                _logger.LogInformation($"Received message: {result.Message.Value}");
                var requestModel = JsonConvert.DeserializeObject<RequestFirebaseNotification>(result.Message.Value);
                // Call the Order Service
                await SendNotification(requestModel).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer shutting down...");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task SendNotification(RequestFirebaseNotification request)
    {
        if (request.FromAccountId != request.ToAccountId)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var accountFrom = accountRepository.GetById(request.FromAccountId);
            var accountTo = accountRepository.GetById(request.ToAccountId);
            var identityRoleName = accountFrom.RoleId == (int)Roles.ShopDelivery ? "nhân viên giao hàng" : (accountFrom.RoleId == (int)Roles.ShopOwner ? "cửa hàng" : "khách hàng");
            var typeNotification = accountFrom.RoleId == (int)Roles.ShopDelivery ? NotificationTypes.SendToShopDeliveryStaff : (accountFrom.RoleId == (int)Roles.ShopOwner ? NotificationTypes.SendToShop : NotificationTypes.SendToCustomer);
            var notification = new Notification()
            {
                AccountId = accountTo.Id,
                Title = "Tin Nhắn",
                Content = $"Bạn vừa có một tin nhắn mới từ {identityRoleName}: {accountFrom.FullName}",
                EntityType = NotificationEntityTypes.Chat,
                ImageUrl = accountFrom.AvatarUrl,
                Type = typeNotification,
                IsSave = false,
            };

            await _mobileNotificationService.NotifyAsync(notification).ConfigureAwait(false);
        }
    }
}