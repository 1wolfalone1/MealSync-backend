using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Queries.GetEvidenceDeliveryFail;

public class GetEvidenceDeliveryFailValidator : AbstractValidator<GetEvidenceDeliveryFailQuery>
{
    public GetEvidenceDeliveryFailValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id đơn hàng");
    }
}