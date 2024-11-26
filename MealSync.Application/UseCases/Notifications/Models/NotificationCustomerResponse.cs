using System.Text.Json.Serialization;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Notifications.Models;

public class NotificationCustomerResponse
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

        [JsonIgnore]
        public DateTimeOffset CreatedDateTime { get; set; }

        [JsonIgnore]
        public DateTimeOffset UpdatedDateTime { get; set; }

        public long CreatedDate
        {
            get
            {
                return CreatedDateTime.ToUnixTimeMilliseconds();
            }
        }

        public long UpdatedDate
        {
            get
            {
                return UpdatedDateTime.ToUnixTimeMilliseconds();
            }
        }
    }
}