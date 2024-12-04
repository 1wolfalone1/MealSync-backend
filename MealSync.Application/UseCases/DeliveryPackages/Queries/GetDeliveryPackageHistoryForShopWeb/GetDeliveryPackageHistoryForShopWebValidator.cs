using FluentValidation;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetDeliveryPackageHistoryForShopWeb;

public class GetDeliveryPackageHistoryForShopWebValidator : AbstractValidator<GetDeliveryPackageHistoryForShopWebQuery>
{
    public GetDeliveryPackageHistoryForShopWebValidator()
    {
        RuleFor(x => x.StatusMode)
            .IsInEnum()
            .WithMessage("Vui lòng cung cấp status mode filter tu 0 - 2");

        When(x => x.DateFrom.HasValue && x.DateTo.HasValue, () =>
        {
            RuleFor(x => x.DateFrom)
                .NotEmpty()
                .WithMessage("Vui lòng cung cấp ngày bắt đầu");

            RuleFor(x => x.DateTo.Value.Date)
                .NotEmpty()
                .WithMessage("Vui lòng cung cấp ngày kết thúc")
                .GreaterThanOrEqualTo(x => x.DateFrom.Value.Date)
                .WithMessage("Ngày from <  to")
                .LessThanOrEqualTo(TimeFrameUtils.GetCurrentDateInUTC7().Date)
                .WithMessage("Dateto phải nhỏ hơn hoặc bằng ngày hiện tại");
        });
    }
}