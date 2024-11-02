using FluentValidation;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetAllDeliveryPackages;

public class GetAllDeliveryPackageValidator : AbstractValidator<GetAllDeliveryPackageQuery>
{
    public GetAllDeliveryPackageValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp ít nhất một status.")
            .ForEach(status => status
                .IsInEnum()
                .WithMessage("Vui lòng cung cấp status từ 1-3"));

        RuleFor(x => x.IntendedReceiveDate)
            .Must(x => x != default)
            .WithMessage("Vui lòng cung cấp ngày giao hàng");

        RuleFor(x => x.StartTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian bắt đầu đúng định dạng hhMM");

        RuleFor(x => x.EndTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian kết thúc đúng định dạng hhMM");
    }
}
