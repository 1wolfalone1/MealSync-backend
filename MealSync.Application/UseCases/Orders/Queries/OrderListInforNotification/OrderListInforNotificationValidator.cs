using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Queries.OrderListInforNotification;

public class OrderListInforNotificationValidator : AbstractValidator<OrderListInforNotificationQuery>
{
    public OrderListInforNotificationValidator()
    {
        RuleFor(x => x.Ids)
            .Must(x => x != null && x.Length > 0)
            .WithMessage("Ít nhất 1 order id");
    }
}