using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Notifications.Models;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Notifications.Queries.GetNotificationForShopAndStaff;

public class GetNotificationForShopAndStaffHandler : IQueryHandler<GetNotificationForShopAndStaffQuery, Result>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetNotificationForShopAndStaffHandler> _logger;

    public GetNotificationForShopAndStaffHandler(INotificationRepository notificationRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper, IUnitOfWork unitOfWork, ILogger<GetNotificationForShopAndStaffHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(GetNotificationForShopAndStaffQuery request, CancellationToken cancellationToken)
    {
        var notificationPaging = _notificationRepository.GetListNotificationBaseOnAccountId(_currentPrincipalService.CurrentPrincipalId.Value, request.PageIndex, request.PageSize);
        var notifications = _mapper.Map<List<NotificationResponse.NotificationInfor>>(notificationPaging.Notifications);

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

        return Result.Success(new PaginationResponse<NotificationResponse.NotificationInfor>(notifications, notificationPaging.TotalCount, request.PageIndex, request.PageSize));
    }
}