using FluentValidation;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.Foods.Queries.GetByIds;

public class GetByIdsForCartValidate : AbstractValidator<GetByIdsForCartQuery>
{
    public GetByIdsForCartValidate()
    {
        RuleFor(x => x.ShopId)
            .GreaterThan(0).WithMessage("Shop id phải lớn hơn 0.");

        RuleFor(x => x.OrderTime)
            .SetValidator(new OrderTimeQueryValidator());

        RuleFor(x => x.Foods)
            .NotEmpty()
            .WithMessage("Đồ ăn kiểm tra bắt buộc");

        RuleForEach(x => x.Foods).ChildRules(food =>
        {
            food.RuleFor(f => f.Id)
                .NotEmpty()
                .WithMessage("Id đồ ăn bắt buộc");

            food.RuleForEach(f => f.OptionGroupRadio).ChildRules(radio =>
            {
                radio.RuleFor(r => r.Id)
                    .GreaterThan(0)
                    .WithMessage("Lựa chọn nhóm id phải lớn hơn 0");

                radio.RuleFor(r => r.OptionId)
                    .GreaterThan(0)
                    .WithMessage("Lựa chọn id phải lớn hơn không");
            });

            food.RuleForEach(f => f.OptionGroupCheckbox).ChildRules(checkbox =>
            {
                checkbox.RuleFor(c => c.Id)
                    .GreaterThan(0)
                    .WithMessage("Lựa chọn nhóm id phải lớn hơn 0");

                checkbox.RuleFor(c => c.OptionIds)
                    .NotEmpty()
                    .WithMessage("Lựa chọn id bắt buộc")
                    .Must(ids => ids.All(id => id > 0))
                    .WithMessage("Lựa chọn ids phải lớn hơn 0");
            });
        });
    }

    public class OrderTimeQueryValidator : AbstractValidator<GetByIdsForCartQuery.OrderTimeQuery>
    {
        public OrderTimeQueryValidator()
        {
            // Validate start and end times (within 24 hours, 0-23 range)
            RuleFor(x => x.StartTime)
                .Must(TimeUtils.IsValidOperatingSlot)
                .WithMessage("Vui lòng cung cấp thời gian bắt đầu đúng định dạng hhMM.");

            RuleFor(x => x)
                .Must(x => x.EndTime > x.StartTime && TimeUtils.IsValidOperatingSlot(x.EndTime) && TimeUtils.IsThirtyMinuteDifference(x.StartTime, x.EndTime))
                .WithMessage($"Thời gian kết thúc bằng thời gian bắt đầu cộng {FrameConstant.TIME_FRAME_IN_MINUTES} phút.");
        }
    }
}