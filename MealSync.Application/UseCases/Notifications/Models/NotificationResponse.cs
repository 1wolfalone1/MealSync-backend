using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Notifications.Models;

public class NotificationResponse
{
    public int TotalUnerad { get; set; }

    public List<NotificationInfor> Notifications { get; set; }

    public class NotificationInfor
    {
        public long Id { get; set; }

        public long AccountId { get; set; }

        public long ReferenceId { get; set; }

        public string? ImageUrl { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string Data { get; set; }

        public NotificationEntityTypes EntityType { get; set; }

        public bool IsRead { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset UpdatedDate { get; set; }
    }
}