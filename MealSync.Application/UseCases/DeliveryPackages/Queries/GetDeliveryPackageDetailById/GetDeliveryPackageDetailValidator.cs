using FluentValidation;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetDeliveryPackageDetailByTimeFrames;

public class GetDeliveryPackageDetailValidator : AbstractValidator<GetDeliveryPackageDetailQuery>
{
    public GetDeliveryPackageDetailValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của gói hàng");
    }
}