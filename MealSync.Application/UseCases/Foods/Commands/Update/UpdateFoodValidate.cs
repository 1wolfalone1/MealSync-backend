using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Commands.Update;

public class UpdateFoodValidate : AbstractValidator<UpdateFoodCommand>
{
    public UpdateFoodValidate()
    {
        RuleFor(p => p.Id)
            .GreaterThan(0)
            .WithMessage("Id sản phẩm phải lớn hơn không.");

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
    }
}