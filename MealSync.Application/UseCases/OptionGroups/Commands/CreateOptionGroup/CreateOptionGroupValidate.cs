using FluentValidation;

namespace MealSync.Application.UseCases.OptionGroups.Commands.CreateOptionGroup;

public class CreateOptionGroupValidate : AbstractValidator<CreateOptionGroupCommand>
{
    public CreateOptionGroupValidate()
    {
        RuleFor(og => og.Title)
            .NotEmpty()
            .WithMessage("Tên nhóm lựa chọn bắt buộc nhập.");

        RuleFor(og => og.IsRequire)
            .NotNull()
            .WithMessage("Lựa chọn có bắt buộc không.");

        RuleFor(og => og.Type)
            .IsInEnum()
            .WithMessage("Nhóm lựa chọn là Radio hay Checkbox.");

        RuleFor(og => og.Options)
            .NotEmpty()
            .WithMessage("Phải có ít nhất một lựa chọn.");

        RuleForEach(og => og.Options)
            .SetValidator(new CreateOptionValidate());
    }

    public class CreateOptionValidate : AbstractValidator<CreateOptionGroupCommand.CreateOptionCommand>
    {
        public CreateOptionValidate()
        {
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
}