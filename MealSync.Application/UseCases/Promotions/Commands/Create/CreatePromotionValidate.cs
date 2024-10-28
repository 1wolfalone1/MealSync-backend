using FluentValidation;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Promotions.Commands.Create;

public class CreatePromotionValidate : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionValidate()
    {
        RuleFor(p => p.ApplyType)
            .IsInEnum().WithMessage("Mã giảm giá áp dụng theo 1(Phần trăm) hoặc 2(Tiền).");

        RuleFor(p => p.Status)
            .Must(p => p == PromotionStatus.Active || p == PromotionStatus.UnActive)
            .WithMessage("Trạng thái phải là 1(Hoạt động) hoặc 2(Không hoạt động).");

        When(p => p.ApplyType == PromotionApplyTypes.Percent, () =>
        {
            RuleFor(p => p.AmountRate)
                .GreaterThan(0).WithMessage("Phần trăm giảm giá phải lớn hơn 0.");

            RuleFor(p => p.MaximumApplyValue)
                .GreaterThan(0).WithMessage("Số tiền giảm giá tối đa phải lớn hơn 0.");

            RuleFor(p => p.AmountValue)
                .Must(v => v == default || v == 0).WithMessage("Số tiền giảm giá không áp dụng cho phầm trăm và phải bằng không 0 hoặc null.");
        });

        When(p => p.ApplyType == PromotionApplyTypes.Absolute, () =>
        {
            RuleFor(p => p.AmountValue)
                .GreaterThan(0).WithMessage("Số tiền giảm giá phải lớn hơn 0.");

            RuleFor(p => p.AmountRate)
                .Must(v => v == default || v == 0).WithMessage("Phần trăm giảm giá không áp dụng cho giảm giá tiền và phải bằng 0 hoặc null.");

            RuleFor(p => p.MaximumApplyValue)
                .Must(v => v == default || v == 0).WithMessage("Số tiền giảm giá tối đa không áp dụng cho giảm giá tiền và phải bằng 0 hoặc null.");
        });

        RuleFor(p => p.Title)
            .NotEmpty().WithMessage("Tiêu đề của mã giảm giá bắt buộc nhập.");

        RuleFor(p => p.MinOrdervalue)
            .GreaterThanOrEqualTo(0).WithMessage("Mã giảm giá áp dụng cho đơn hàng tối thiểu lớn hơn hoặc bằng 0.");

        RuleFor(p => p.StartDate)
            .LessThan(p => p.EndDate).WithMessage("Ngày bắt đầu phải sớm hơn ngày kết thúc.");

        RuleFor(p => p.UsageLimit)
            .GreaterThan(0).WithMessage("Số lượng giới hạn phải lớn hơn 0.");

    }
}