using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Notifications.Models;
using MealSync.Application.UseCases.Notifications.Queries.GetCustomerNotification;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Notifications.Queries.GetModeratorNotification;

public class GetModeratorNotificationHandler : IQueryHandler<GetModeratorNotificationQuery, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<GetCustomerNotificationHandler> _logger;

    public GetModeratorNotificationHandler(IUnitOfWork unitOfWork, INotificationRepository notificationRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService, ILogger<GetCustomerNotificationHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationRepository = notificationRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(GetModeratorNotificationQuery request, CancellationToken cancellationToken)
    {
        var notificationPaging = _notificationRepository.GetListNotificationBaseOnAccountId(_currentPrincipalService.CurrentPrincipalId.Value, request.PageIndex, request.PageSize);
        var notifications = _mapper.Map<List<NotificationCustomerResponse.NotificationInfor>>(notificationPaging.Notifications);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            foreach (var notification in notificationPaging.Notifications)
            {
                notification.IsRead = true;
            }

            _notificationRepository.UpdateRange(notificationPaging.Notifications);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        return Result.Success(new PaginationResponse<NotificationCustomerResponse.NotificationInfor>(notifications, notificationPaging.TotalCount, request.PageIndex, request.PageSize));
    }
}