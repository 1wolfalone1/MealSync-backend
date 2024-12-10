using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Services.Chat;

public class AddChat
{
    public long RoomId { get; set; }

    public bool IsOpen { get; set; }

    public long UserId { get; set; }

    public Notification Notification { get; set; }
}