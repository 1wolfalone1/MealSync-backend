using FluentValidation;
using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.Orders.Queries.ShopOrderByStatus;

namespace MealSync.Application.UseCases.Orders.Queries.ShopWebOrderByStatus;

public class GetShopWebOrderByStatusValidator : AbstractValidator<GetShopWebOrderByStatusQuery>
{
    public GetShopWebOrderByStatusValidator()
    {
        RuleFor(x => x.Status)
            .ForEach(x => x.IsInEnum().WithMessage("Vui lòng cung cấp danh sách status từ 1 đến 12"));

        When(x => x.IntendedReceiveDate.HasValue, () =>
        {
            RuleFor(x => x.DateFrom)
                .Must(x => !x.HasValue)
                .WithMessage("Vui lòng không truyền ngày nhận hàng để filter trong 1 khoảng thời gian");

            RuleFor(x => x.DateTo)
                .Must(x => !x.HasValue)
                .WithMessage("Vui lòng không truyền ngày nhận hàng để filter trong 1 khoảng thời gian");
        });

        When(x => x.DateFrom.HasValue && x.DateTo.HasValue, () =>
        {
            RuleFor(x => x.DateFrom)
                .Must(x => x.Value <= TimeFrameUtils.GetCurrentDateInUTC7().Date)
                .WithMessage("Vui lòng cung cấp ngày bắt đầu nhỏ hơn hoặc bẳng ngày hiện tại")
                .LessThanOrEqualTo(x => x.DateTo.Value)
                .WithMessage("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");

            RuleFor(x => x.DateTo)
                .Must(x => x.Value <= TimeFrameUtils.GetCurrentDateInUTC7().Date)
                .WithMessage("Vui lòng cung cấp ngày kết thúc nhỏ hơn hoặc bẳng ngày hiện tại");
        });
    }
}