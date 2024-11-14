using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Queries.GetDeliveryInfoFail;

public class GetDeliveryInforValidator : AbstractValidator<GetDeliveryInfoQuery>
{
    public GetDeliveryInforValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id đơn hàng");
    }
}