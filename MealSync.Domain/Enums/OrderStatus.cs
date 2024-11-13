namespace MealSync.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Rejected = 2,
    Confirmed = 3,
    Cancelled = 4,
    Preparing = 5,
    Delivering = 6,
    Delivered = 7,
    FailDelivery = 8,
    Completed = 9,
    IssueReported = 10,
    UnderReview = 11,
    Resolved = 12,
    PendingPayment = 13,
}