using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MealSync.Infrastructure.Services.Notifications;

public class NotifierSerivce : INotifierSerivce
{
    private readonly INotificationProvider _notificationProvider;
    private readonly ILogger<NotifierSerivce> _logger;
    private readonly IMobileNotificationService _mobileNotificationService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NotifierSerivce(INotificationProvider notificationProvider, ILogger<NotifierSerivce> logger, IMobileNotificationService mobileNotificationService, IServiceScopeFactory serviceScopeFactory)
    {
        _notificationProvider = notificationProvider;
        _logger = logger;
        _mobileNotificationService = mobileNotificationService;
        _serviceScopeFactory = serviceScopeFactory;

        _notificationProvider.Attach(NotificationTypes.SendToCustomer, new List<INotificationService>()
        {
            _mobileNotificationService,
        });

        _notificationProvider.Attach(NotificationTypes.SendToShop, new List<INotificationService>()
        {
            _mobileNotificationService,
        });

        _notificationProvider.Attach(NotificationTypes.SendToModerator, new List<INotificationService>()
        {
        });

        _notificationProvider.Attach(NotificationTypes.SendToAdmin, new List<INotificationService>()
        {
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
}