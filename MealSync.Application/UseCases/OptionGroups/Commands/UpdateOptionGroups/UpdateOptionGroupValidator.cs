using FluentValidation;
using MealSync.Application.UseCases.OptionGroups.Commands.CreateOptionGroup;

namespace MealSync.Application.UseCases.OptionGroups.Commands.UpdateOptionGroups;

public class UpdateOptionGroupValidator : AbstractValidator<UpdateOptionGroupCommand>
{
    public UpdateOptionGroupValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của câu hỏi");

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
            .SetValidator(new UpdateOptionValidator());
    }
}

public class UpdateOptionValidator : AbstractValidator<UpdateOptionRequest>
{
    public UpdateOptionValidator()
    {
        RuleFor(o => o.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của lựa chọn");

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