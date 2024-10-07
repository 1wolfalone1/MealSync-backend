using System.ComponentModel;

namespace MealSync.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Rejected = 2,
    Confirmed = 3,
    Cancelled = 4,
    Preparing = 5,
    Delivering = 6,
    Completed = 7,
    FailDelivery = 8,
    IssueReported = 9,
    UnderReview = 10,
    Resolved = 11,
}