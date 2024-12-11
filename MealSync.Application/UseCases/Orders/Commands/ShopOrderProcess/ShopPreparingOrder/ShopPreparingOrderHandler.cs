using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Chat;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopPreparingOrder;

public class ShopPreparingOrderHandler : ICommandHandler<ShopPreparingOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<Result> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly INotifierService _notifierService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IShopRepository _shopRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IChatService _chatService;
    private readonly IAccountRepository _accountRepository;

    public ShopPreparingOrderHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, ILogger<Result> logger, ICurrentPrincipalService currentPrincipalService, INotifierService notifierService,
        INotificationFactory notificationFactory, IShopRepository shopRepository, ISystemResourceRepository systemResourceRepository, IChatService chatService, IAccountRepository accountRepository)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _notifierService = notifierService;
        _notificationFactory = notificationFactory;
        _shopRepository = shopRepository;
        _systemResourceRepository = systemResourceRepository;
        _chatService = chatService;
        _accountRepository = accountRepository;
    }

    public async Task<Result<Result>> Handle(ShopPreparingOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var order = _orderRepository.Get(o => o.Id == request.Id)
            .Include(o => o.Customer).Single();

        if (!request.IsConfirm.Value)
        {
            if (order.IntendedReceiveDate.Date != TimeFrameUtils.GetCurrentDateInUTC7().Date)
            {
                return Result.Warning(new
                {
                    Code = MessageCode.W_ORDER_NOT_IN_DATE_PREPARING.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_NOT_IN_DATE_PREPARING.GetDescription(), order.Id),
                });
            }
            else
            {
                var now = TimeFrameUtils.GetCurrentDateInUTC7();
                var intendedReceiveDateTime = new DateTime(
                    order.IntendedReceiveDate.Year,
                    order.IntendedReceiveDate.Month,
                    order.IntendedReceiveDate.Day,
                    order.StartTime / 100,
                    order.StartTime % 100,
                    0);
                var endTime = new DateTimeOffset(intendedReceiveDateTime, TimeSpan.FromHours(7)).AddHours(-OrderConstant.TIME_WARNING_SHOP_PREPARE_ORDER_EARLY_IN_HOURS);
                if (now < endTime)
                {
                    var diffDate = endTime.AddHours(OrderConstant.TIME_WARNING_SHOP_PREPARE_ORDER_EARLY_IN_HOURS) - now;
                    return Result.Warning(new
                    {
                        Code = MessageCode.W_ORDER_PREPARING_EARLY.GetDescription(),
                        Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_PREPARING_EARLY.GetDescription(),
                            new string[] { order.Id.ToString(), TimeFrameUtils.GetTimeFrameString(order.StartTime, order.EndTime), $"{diffDate.Hours}:{diffDate.Minutes}" }),
                    });
                }
            }
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            order.Status = OrderStatus.Preparing;
            _orderRepository.Update(order);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            // Noti for customer
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
            var noti = _notificationFactory.CreateOrderPreparingNotification(order, shop);
            _notifierService.NotifyAsync(noti);

            var shopAccount = _accountRepository.GetById(order.ShopId);
            var notificationJoinRoom = _notificationFactory.CreateJoinRoomToCustomerNotification(order, shopAccount);

            _chatService.OpenOrCloseRoom(new AddChat()
            {
                IsOpen = true,
                RoomId = order.Id,
                UserId = order.CustomerId,
                Notification = null,
            });

            _chatService.OpenOrCloseRoom(new AddChat()
            {
                IsOpen = true,
                RoomId = order.Id,
                UserId = order.ShopId,
                Notification = notificationJoinRoom,
            });

            return Result.Success(new
            {
                Code = MessageCode.I_ORDER_SHOP_CHANGE_PREPARING_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_SHOP_CHANGE_PREPARING_SUCCESS.GetDescription(), order.Id),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(ShopPreparingOrderCommand request)
    {
        var order = _orderRepository.Get(o => o.Id == request.Id && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Confirmed)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { request.Id });

        var currentDateTime = TimeFrameUtils.GetCurrentDateInUTC7();
        var startEndTime = TimeFrameUtils.GetStartTimeEndTimeToDateTime(order.IntendedReceiveDate, order.StartTime, order.EndTime);
        if (currentDateTime.DateTime > startEndTime.EndTime)
            throw new InvalidBusinessException(MessageCode.E_ORDER_OVER_TIME.GetDescription(), new object[] { request.Id });
    }

}