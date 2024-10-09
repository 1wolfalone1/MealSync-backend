using FluentValidation;

namespace MealSync.Application.UseCases.Options.Commands.CreateNewOption;

public class CreateNewOptionValidator : AbstractValidator<CreateNewOptionCommand>
{
    public CreateNewOptionValidator()
    {
        RuleFor(o => o.OptionGroupId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của câu hỏi lựa chọn");

        RuleFor(o => o.IsDefault)
            .NotNull()
            .WithMessage("Lựa chọn có phải mặc định không.");

        RuleFor(o => o.Title)
            .NotEmpty()
            .WithMessage("Tên lựa chọn bắt buộc nhập.");

        RuleFor(o => o.IsCalculatePrice)
            .NotNull()
            .WithMessage("Lựa chọn có tính tiền không.");

        RuleFor(o => o.Price)
            .Must((o, price) =>
                (o.IsCalculatePrice && price > 0) || (!o.IsCalculatePrice && price == 0))
            .WithMessage("Giá phải lớn hơn 0 nếu lựa chọn có tính tiền, và bằng 0 nếu không có tính tiền.");
    }
}