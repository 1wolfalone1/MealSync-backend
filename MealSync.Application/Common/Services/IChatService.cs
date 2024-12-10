using MealSync.Application.Common.Services.Chat;

namespace MealSync.Application.Common.Services;

public interface IChatService
{
    Task OpenOrCloseRoom(AddChat addChat);
}