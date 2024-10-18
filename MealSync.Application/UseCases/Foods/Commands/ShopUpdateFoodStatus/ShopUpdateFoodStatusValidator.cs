using FluentValidation;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Commands.ShopUpdateFoodStatus;

public class ShopUpdateFoodStatusValidator : AbstractValidator<ShopUpdateFoodStatusCommand>
{
    public ShopUpdateFoodStatusValidator()
    {
        RuleFor(f => f.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của món ăn");

        RuleFor(f => f.IsSoldOut)
            .NotNull()
            .WithMessage("Vui lòng cung là món ăn có tạm ngưng bán hay không");

        RuleFor(f => f.Status)
            .Must(s => s == FoodStatus.Active || s == FoodStatus.UnActive)
            .WithMessage("Vui lòng cung cấp đúng trạng thái món ăn cần đổi 1 (Hoạt động) | 2 (Ngưng hoạt động)");

        When(f => f.Status == FoodStatus.UnActive, () =>
        {
            RuleFor(f => f.IsSoldOut)
                .Must(f => f.Value)
                .WithMessage("Không tồn tại trường hợp món ăn tạm ẩn và tạm ngưng bán");
        });
    }
}