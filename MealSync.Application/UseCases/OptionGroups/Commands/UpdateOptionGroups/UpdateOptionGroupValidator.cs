using FluentValidation;
using MealSync.Application.UseCases.OptionGroups.Commands.CreateOptionGroup;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.OptionGroups.Commands.UpdateOptionGroups;

public class UpdateOptionGroupValidator : AbstractValidator<UpdateOptionGroupCommand>
{
    public UpdateOptionGroupValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của nhóm lựa chọn");

        RuleFor(og => og.Title)
            .NotEmpty()
            .WithMessage("Tên nhóm lựa chọn bắt buộc nhập.");

        RuleFor(og => og.IsRequire)
            .NotNull()
            .WithMessage("Lựa chọn có bắt buộc không.");

        RuleFor(og => og.Type)
            .IsInEnum()
            .WithMessage("Nhóm lựa chọn là Radio hay Checkbox.");

             // Ensure MaxChoices >= MinChoices for all cases
        RuleFor(og => og.MaxChoices)
            .GreaterThanOrEqualTo(og => og.MinChoices)
            .WithMessage("MaxChoices phải lớn hơn hoặc bằng MinChoices.");

        // Validate when IsRequire = true and Type = Radio
        When(og => og.IsRequire && og.Type == OptionGroupTypes.Radio, () =>
        {
            RuleFor(og => og.MinChoices)
                .Equal(1)
                .WithMessage("MinChoices phải bằng 1 khi bắt buộc và loại là Radio.");

            RuleFor(og => og.MaxChoices)
                .Equal(1)
                .WithMessage("MaxChoices phải bằng 1 khi bắt buộc và loại là Radio.");
        });

        // Validate when IsRequire = true and Type = Checkbox
        When(og => og.IsRequire && og.Type == OptionGroupTypes.CheckBox, () =>
        {
            RuleFor(og => og.MinChoices)
                .Equal(1)
                .WithMessage("MinChoices phải bằng 1 khi bắt buộc và loại là Checkbox.");

            RuleFor(og => og.MaxChoices)
                .GreaterThanOrEqualTo(1)
                .WithMessage("MaxChoices phải lớn hơn hoặc bằng 1 khi bắt buộc và loại là Checkbox.");
        });

        // Validate when IsRequire = false and Type = Radio
        When(og => !og.IsRequire && og.Type == OptionGroupTypes.Radio, () =>
        {
            RuleFor(og => og.MinChoices)
                .Equal(0)
                .WithMessage("MinChoices phải bằng 0 khi không bắt buộc và loại là Radio.");

            RuleFor(og => og.MaxChoices)
                .Equal(1)
                .WithMessage("MaxChoices phải bằng 1 khi không bắt buộc và loại là Radio.");
        });

        // Validate when IsRequire = false and Type = Checkbox
        When(og => !og.IsRequire && og.Type == OptionGroupTypes.CheckBox, () =>
        {
            RuleFor(og => og.MinChoices)
                .Equal(0)
                .WithMessage("MinChoices phải bằng 0 khi không bắt buộc và loại là Checkbox.");

            RuleFor(og => og.MaxChoices)
                .GreaterThanOrEqualTo(1)
                .WithMessage("MaxChoices phải lớn hơn hoặc bằng 1 khi không bắt buộc và loại là Checkbox.");
        });

        RuleFor(og => og.Status)
            .Must(status => status == OptionGroupStatus.Active || status == OptionGroupStatus.UnActive)
            .WithMessage("Trạng thái của lựa chọn nhóm chỉ được là 'Active' hoặc 'Inactive'.");

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

        RuleFor(og => og.Status)
            .Must(status => status == OptionStatus.Active || status == OptionStatus.UnActive)
            .WithMessage("Trạng thái của lựa chọn chỉ được là 'Active' hoặc 'Inactive'.");
    }
}