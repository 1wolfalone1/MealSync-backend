using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Chat;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Test.Commands.TestOpenRoom;

public class TestOpenRoomHandler : ICommandHandler<TestOpenRoomCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IChatService _chatService;
    private readonly IAccountRepository _accountRepository;
    private readonly INotificationFactory _notificationFactory;

    public TestOpenRoomHandler(IOrderRepository orderRepository, IChatService chatService, IAccountRepository accountRepository, INotificationFactory notificationFactory)
    {
        _orderRepository = orderRepository;
        _chatService = chatService;
        _accountRepository = accountRepository;
        _notificationFactory = notificationFactory;
    }

    public async Task<Result<Result>> Handle(TestOpenRoomCommand request, CancellationToken cancellationToken)
    {
        var order = _orderRepository.GetById(request.OrderId);
        var accountIds = _orderRepository.GetListAccountIdRelatedToOrder(request.OrderId);
        var idsDistinct = accountIds.Distinct();
        var accounts = _accountRepository.GetAccountByIds(idsDistinct.ToList());
        foreach (var account in accounts)
        {
            if (account.RoleId == (int)Domain.Enums.Roles.ShopOwner)
            {
                var notificationJoinRoom = _notificationFactory.CreateJoinRoomToCustomerNotification(order, account);

                _chatService.OpenOrCloseRoom(new AddChat()
                {
                    IsOpen = true,
                    RoomId = order.Id,
                    UserId = account.Id,
                    Notification = notificationJoinRoom,
                });
            }

            if (account.RoleId == (int)Domain.Enums.Roles.ShopDelivery)
            {
                var notificationJoinRoom = _notificationFactory.CreateJoinRoomToCustomerNotification(order, account);

                _chatService.OpenOrCloseRoom(new AddChat()
                {
                    IsOpen = true,
                    RoomId = order.Id,
                    UserId = account.Id,
                    Notification = notificationJoinRoom,
                });
            }

            if (account.RoleId == (int)Domain.Enums.Roles.Customer)
            {
                _chatService.OpenOrCloseRoom(new AddChat()
                {
                    IsOpen = true,
                    RoomId = order.Id,
                    UserId = account.Id,
                    Notification = null,
                });
            }
        }

        return Result.Success();
    }
}