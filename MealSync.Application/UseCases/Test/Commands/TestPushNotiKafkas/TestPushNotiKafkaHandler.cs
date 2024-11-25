using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Test.Commands.TestPushNotiKafkas;

public class TestPushNotiKafkaHandler : ICommandHandler<TestPushNotiKafkaCommand, Result>
{
    private readonly IWebNotificationService _webNotificationService;

    public TestPushNotiKafkaHandler(IWebNotificationService webNotificationService)
    {
        _webNotificationService = webNotificationService;
    }

    public async Task<Result<Result>> Handle(TestPushNotiKafkaCommand request, CancellationToken cancellationToken)
    {
        var noti = new Notification()
        {
            AccountId = 3,
            ReferenceId = 1,
            Content = request.Message,
            Data = string.Empty,
            Type = NotificationTypes.SendToCustomer,
            ImageUrl = "https://thanhtu-blog.s3.ap-southeast-1.amazonaws.com/image/eb7ce841-6579-458a-a46a-1dfc8491ed81-1727165769091.png",
            Title = "TEST NOTIFICATION",
            EntityType = NotificationEntityTypes.Order,
        };
        await _webNotificationService.NotifyAsync(noti).ConfigureAwait(false);
        return Result.Success();
    }
}