using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Notifications.Models;
using MediatR;

namespace MealSync.Application.UseCases.Notifications.Queries.GetTotalUnreadNotification;

public class GetTotalUnreadNotifcationHandler : IQueryHandler<GetTotalUnreadNotificationQuery, Result>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetTotalUnreadNotifcationHandler(ICurrentPrincipalService currentPrincipalService, INotificationRepository notificationRepository, IMapper mapper)
    {
        _currentPrincipalService = currentPrincipalService;
        _notificationRepository = notificationRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetTotalUnreadNotificationQuery request, CancellationToken cancellationToken)
    {
        var notificationPaging = _notificationRepository.GetListNotificationBaseOnAccountId(_currentPrincipalService.CurrentPrincipalId.Value, request.PageIndex, request.PageSize);
        var notifications = _mapper.Map<List<NotificationResponse.NotificationInfor>>(notificationPaging.Notifications);

        return Result.Success(new NotificationResponse()
        {
            TotalUnerad = notificationPaging.TotalUnread,
            Notifications = notifications
        });
    }
}