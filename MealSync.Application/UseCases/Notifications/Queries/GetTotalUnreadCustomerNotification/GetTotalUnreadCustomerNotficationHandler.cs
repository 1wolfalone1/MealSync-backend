using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Notifications.Queries.GetTotalUnreadCustomerNotification;

public class GetTotalUnreadCustomerNotficationHandler : IQueryHandler<GetTotalUnreadCustomerNotificationQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly INotificationRepository _notificationRepository;

    public GetTotalUnreadCustomerNotficationHandler(ICurrentPrincipalService currentPrincipalService, INotificationRepository notificationRepository)
    {
        _currentPrincipalService = currentPrincipalService;
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<Result>> Handle(GetTotalUnreadCustomerNotificationQuery request, CancellationToken cancellationToken)
    {
        var notificationPaging = _notificationRepository.GetListNotificationBaseOnAccountId(_currentPrincipalService.CurrentPrincipalId.Value, request.PageIndex, request.PageSize);
        return Result.Success(new
        {
            TotalUnRead = notificationPaging.TotalUnread,
        });
    }
}