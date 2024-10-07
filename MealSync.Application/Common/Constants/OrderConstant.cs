using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Constants;

public static class OrderConstant
{
    public static readonly List<OrderStatus> LIST_ORDER_STATUS_IN_PROCESSING = new List<OrderStatus>()
    {
        OrderStatus.Pending,
        OrderStatus.Confirmed,
        OrderStatus.Preparing,
        OrderStatus.Delivering,
    };
}