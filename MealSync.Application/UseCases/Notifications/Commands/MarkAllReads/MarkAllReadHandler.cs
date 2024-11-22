using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Notifications.Commands.MarkAllReads;

public class MarkAllReadHandler : ICommandHandler<MarkAllReadCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ILogger<MarkAllReadHandler> _logger;

    public MarkAllReadHandler(IUnitOfWork unitOfWork, INotificationRepository notificationRepository, ICurrentPrincipalService currentPrincipalService, ILogger<MarkAllReadHandler> logger, ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _notificationRepository = notificationRepository;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(MarkAllReadCommand request, CancellationToken cancellationToken)
    {
        var notification = _notificationRepository.GetNotificationUnRead(_currentPrincipalService.CurrentPrincipalId.Value);
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            notification.ForEach(n => n.IsRead = true);
            _notificationRepository.UpdateRange(notification);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        return Result.Success(new
        {
            Message = _systemResourceRepository.GetByResourceCode(MessageCode.E_NOTIFICATION_MARL_ALL_READ_SUCCESS.GetDescription()),
            Code = MessageCode.E_NOTIFICATION_MARL_ALL_READ_SUCCESS.GetDescription(),
        });
    }
}