using FluentValidation;
using MealSync.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace MealSync.Application.UseCases.Reviews.Commands.ReviewOrderOfCustomer;

public class ReviewOrderOfCustomerValidate : AbstractValidator<ReviewOrderOfCustomerCommand>
{
    public ReviewOrderOfCustomerValidate()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Id phải lớn hơn 0");

        RuleFor(x => x.Rating)
            .IsInEnum().WithMessage("Đánh giá phải từ 1 tới 5 sao");

        RuleFor(x => x.Comment)
            .MaximumLength(800).WithMessage("Đánh giá tối đa 800 kí tự.");

        RuleFor(x => x.Images)
            .Must(images => images == null || images.Length <= 5)
            .WithMessage("Tối đa 5 ảnh")
            .When(images => images != default && images.Images != default && images.Images.Length > 0)
            .ForEach(image =>
                image.Must(file => file.Length <= 5 * 1024 * 1024)
                    .WithMessage("Ảnh không được vượt quá 5 MB.")
            );
    }
}