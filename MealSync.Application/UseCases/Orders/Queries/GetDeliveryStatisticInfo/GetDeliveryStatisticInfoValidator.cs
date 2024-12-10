using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Queries.GetDeliveryStatisticInfo;

public class GetDeliveryStatisticInfoValidator : AbstractValidator<GetDeliveryStatisticInfoQuery>
{
    public GetDeliveryStatisticInfoValidator()
    {
    }
}