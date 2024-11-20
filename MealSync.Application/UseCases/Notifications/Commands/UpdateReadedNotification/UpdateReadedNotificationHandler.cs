using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Notifications.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Notifications.Commands.UpdateReadedNotification;

public class UpdateReadedNotificationHandler : ICommandHandler<UpdateReadedNotificationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<UpdateReadedNotificationHandler> _logger;
    private readonly IMapper _mapper;

    public UpdateReadedNotificationHandler(IUnitOfWork unitOfWork, INotificationRepository notificationRepository, ICurrentPrincipalService currentPrincipalService, ILogger<UpdateReadedNotificationHandler> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _notificationRepository = notificationRepository;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(UpdateReadedNotificationCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var notifications = _notificationRepository.GetByIds(_currentPrincipalService.CurrentPrincipalId.Value, request.Ids);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            _notificationRepository.UpdateRange(notifications);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        var notificationResponse = _mapper.Map<List<NotificationResponse.NotificationInfor>>(notifications);
        return Result.Success(notificationResponse);
    }

    private void Validate(UpdateReadedNotificationCommand request)
    {
        var notifications = _notificationRepository.GetByIds(_currentPrincipalService.CurrentPrincipalId.Value, request.Ids);
        var idDbs = notifications.Select(n => n.Id).ToList();
        var listIdsDiff = request.Ids.Except(idDbs).ToList();
        var idMessage = string.Join(", ", listIdsDiff.Select(id => string.Concat(IdPatternConstant.PREFIX_ID, id)));

        if (notifications.Count() != request.Ids.Count())
            throw new InvalidBusinessException(MessageCode.E_NOTIFICATION_NOT_FOUND.GetDescription(), new object[] { idMessage });
    }
}