using FluentValidation;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetAllDeliveryPackageGroupByTimeFrames;

public class GetAllDeliveryPackageGroupByTimeFrameValidator : AbstractValidator<GetAllDeliveryPackageGroupByTimeFrameQuery>
{
    public GetAllDeliveryPackageGroupByTimeFrameValidator()
    {
        RuleFor(x => x.StartTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian bắt đầu đúng định dạng hhMM");

        RuleFor(x => x.EndTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian kết thúc đúng định dạng hhMM");

        RuleFor(x => x.IntendedRecieveDate)
            .Must(x => x != default)
            .WithMessage("Vui lòng cung cấp ngày giao hàng");
    }
}