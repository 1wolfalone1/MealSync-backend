using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MealSync.Infrastructure.Services.Notifications;

public class NotifierService : INotifierService
{
    private readonly INotificationProvider _notificationProvider;
    private readonly ILogger<NotifierService> _logger;
    private readonly IMobileNotificationService _mobileNotificationService;
    private readonly IWebNotificationService _webNotificationService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NotifierService(INotificationProvider notificationProvider, ILogger<NotifierService> logger, IMobileNotificationService mobileNotificationService, IServiceScopeFactory serviceScopeFactory, IWebNotificationService webNotificationService)
    {
        _notificationProvider = notificationProvider;
        _logger = logger;
        _mobileNotificationService = mobileNotificationService;
        _serviceScopeFactory = serviceScopeFactory;
        _webNotificationService = webNotificationService;

        _notificationProvider.Attach(NotificationTypes.SendToCustomer, new List<INotificationService>()
        {
            _mobileNotificationService,
        });

        _notificationProvider.Attach(NotificationTypes.SendToShop, new List<INotificationService>()
        {
            _mobileNotificationService,
            _webNotificationService,
        });

        _notificationProvider.Attach(NotificationTypes.SendToModerator, new List<INotificationService>()
        {
            _webNotificationService,
        });

        _notificationProvider.Attach(NotificationTypes.SendToAdmin, new List<INotificationService>()
        {
            _webNotificationService,
        });
    }

    public async Task NotifyAsync(Notification notification)
    {
        if (notification.IsSave)
        {
            // persist notification into database
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                await unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                await notificationRepository.AddAsync(notification).ConfigureAwait(false);
                await unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        _logger.LogInformation("[PUSH NOTIFICATION]: {0}", JsonConvert.SerializeObject(notification));
        await _notificationProvider.NotifyAsync(notification).ConfigureAwait(false);
    }

    public async Task NotifyRangeAsync(List<Notification> notifications)
    {
        var listNotificationSave = notifications.Where(noti => noti.IsSave).ToList();

        // persist notification into database
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            await unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            await notificationRepository.AddRangeAsync(listNotificationSave).ConfigureAwait(false);
            await unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }

        foreach (var notification in notifications)
        {
            _logger.LogInformation("[PUSH NOTIFICATION]: {0}", JsonConvert.SerializeObject(notifications));
            await _notificationProvider.NotifyAsync(notification).ConfigureAwait(false);
        }
    }
}