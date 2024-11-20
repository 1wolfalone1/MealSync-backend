using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;

namespace MealSync.Application.UseCases.Test.Commands.TestFirebase;

public class TestFirebaseHandler : ICommandHandler<TestFirebaseCommand, Result>
{
    private readonly IMobileNotificationService _mobileNotificationService;

    public TestFirebaseHandler(IMobileNotificationService mobileNotificationService)
    {
        _mobileNotificationService = mobileNotificationService;
    }

    public async Task<Result<Result>> Handle(TestFirebaseCommand request, CancellationToken cancellationToken)
    {
        await _mobileNotificationService.NotifyAsync(new Notification()
        {
            AccountId = 3,
            Title = "test",
            Content = request.Message,
            ReferenceId = 1,
        });

        return Result.Success();
    }
}