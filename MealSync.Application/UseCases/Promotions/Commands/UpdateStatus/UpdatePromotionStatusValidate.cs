using FluentValidation;

namespace MealSync.Application.UseCases.Promotions.Commands.UpdateStatus;

public class UpdatePromotionStatusValidate : AbstractValidator<UpdatePromotionStatusCommand>
{
    public UpdatePromotionStatusValidate()
    {
        RuleFor(q => q.Id)
            .GreaterThan(0)
            .WithMessage("Id phải lớn hơn 0");

        RuleFor(p => p.Status)
            .IsInEnum()
            .WithMessage("Trạng thái phải là 1(Hoạt động) hoặc 2(Không hoạt động) hoặc 3(Xóa).");
    }
}