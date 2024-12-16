using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Chat;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.Schedulers.AutoCloseChatAfterTwoHour;

public class AutoCloseChatAfterTwoHourHandler : ICommandHandler<AutoCloseChatAfterTwoHourCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly INotificationFactory _notificationFactory;
    private readonly IChatService _chatService;
    private readonly IBatchHistoryRepository _batchHistoryRepository;
    private readonly ILogger<AutoCloseChatAfterTwoHourHandler> _logger;

    public AutoCloseChatAfterTwoHourHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IAccountRepository accountRepository, INotificationFactory notificationFactory, IChatService chatService, IBatchHistoryRepository batchHistoryRepository, ILogger<AutoCloseChatAfterTwoHourHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _accountRepository = accountRepository;
        _notificationFactory = notificationFactory;
        _chatService = chatService;
        _batchHistoryRepository = batchHistoryRepository;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(AutoCloseChatAfterTwoHourCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var totalRecord = 0;
        var startTime = TimeFrameUtils.GetCurrentDate();
        var endTime = TimeFrameUtils.GetCurrentDate();

        var orders = _orderRepository.GetListOrderOverTwoHour(TimeFrameUtils.GetDateTimeInUTC7Round());
        foreach (var order in orders)
        {
            var accountIds = _orderRepository.GetListAccountIdRelatedToOrder(order.Id);
            var idsDistinct = accountIds.Distinct();
            var accounts = _accountRepository.GetAccountByIds(idsDistinct.ToList());
            var shopAccount = accounts.Where(a => a.RoleId == (int)Domain.Enums.Roles.ShopOwner).FirstOrDefault();
            if (shopAccount != null)
            {
                var notificationCloseRoom = _notificationFactory.CreateCloseRoomToCustomerNotification(order, shopAccount);
                _chatService.OpenOrCloseRoom(new AddChat()
                {
                    IsOpen = false,
                    RoomId = order.Id,
                    UserId = shopAccount.Id,
                    Notification = notificationCloseRoom,
                });
            }

            totalRecord++;
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var batchHistory = new BatchHistory()
            {
                BatchCode = BatchCodes.BatchCheduleCloseRoomChat,
                Parameter = string.Empty,
                TotalRecord = totalRecord,
                ErrorLog = string.Join(", ", errors),
                StartDateTime = startTime,
                EndDateTime = endTime,
            };

            await _batchHistoryRepository.AddAsync(batchHistory).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Success(batchHistory);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }
}