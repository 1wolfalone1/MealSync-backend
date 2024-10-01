using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Commands.Create;

public class CreateFoodValidate : AbstractValidator<CreateFoodCommand>
{
    public CreateFoodValidate()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage("Tên sản phẩm bắt buộc");

        RuleFor(p => p.Price)
            .NotNull()
            .WithMessage("Giá sản phẩm bắt buộc")
            .GreaterThan(0)
            .WithMessage("Giá sản phẩm phải lớn hơn 0");

        RuleFor(p => p.ImgUrl)
            .NotEmpty()
            .WithMessage("Ảnh sản phẩm bắt buộc");

        RuleFor(p => p.PlatformCategoryId)
            .GreaterThan(0)
            .WithMessage("Sản phẩm phải có ít nhất một thể loại của nền tảng.");

        RuleFor(p => p.ShopCategoryId)
            .GreaterThan(0)
            .WithMessage("Sản phẩm phải có ít nhất một thể loại của shop.");

        RuleFor(p => p.OperatingSlots)
            .NotEmpty()
            .WithMessage("Sản phẩm phải có ít nhất một thời gian hoạt động.");
    }
}