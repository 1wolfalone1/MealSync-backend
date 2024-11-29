using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Services.Notifications.Models;

public class ReportNotification
{
    public long Id { get; set; }

    public long? ShopId { get; set; }

    public long? CustomerId { get; set; }

    public long? ShopDeliveryStaffId { get; set; }

    public long OrderId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public string ImageUrl { get; set; }

    public ReportStatus Status { get; set; }

    public string? Reason { get; set; }
}