using FluentValidation;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.Products.Commands.Create;

public class CreateProductValidate : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidate()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage("Tên sản phẩm bắt buộc");

        RuleFor(p => p.ImgUrl)
            .NotEmpty()
            .WithMessage("Ảnh sản phẩm bắt buộc");

        RuleFor(p => p.Price)
            .NotNull()
            .WithMessage("Giá sản phẩm bắt buộc")
            .GreaterThan(0)
            .WithMessage("Giá sản phẩm phải lớn hơn 0");

        RuleFor(p => p.CategoryIds)
            .NotEmpty()
            .WithMessage("Sản phẩm phải có ít nhất một thể loại.");

        RuleFor(p => p.OperatingHours)
            .NotEmpty()
            .WithMessage("Sản phẩm phải có ít nhất một thời gian hoạt động.");

        RuleForEach(p => p.OperatingHours)
            .SetValidator(new OperatingHourValidate());

        RuleForEach(p => p.Questions)
            .SetValidator(new CreateQuestionValidate());
    }

    public class OperatingHourValidate : AbstractValidator<CreateProductCommand.OperatingHourCommand>
    {
        public OperatingHourValidate()
        {
            RuleFor(q => q.OperatingDayId)
                .GreaterThan(0)
                .WithMessage("OperatingDateId phải lớn hơn 0");

            RuleFor(q => q.StartTime)
                .Must(TimeUtils.IsValidTime)
                .WithMessage("Thời gian bắt đầu phải ở định dạng HHmm, có giờ hợp lệ (00-23) và phút (00-59).");

            RuleFor(q => q.EndTime)
                .Must(TimeUtils.IsValidTime)
                .WithMessage("Thời gian kết thúc phải ở định dạng HHmm, có giờ hợp lệ (00-23) và phút (00-59).");

            // Validate that EndTime is strictly greater than StartTime and in 30-minute increments
            RuleFor(q => q.EndTime)
                .Must((command, endTime) => TimeUtils.IsValidEndTime(command.StartTime, endTime))
                .WithMessage("Thời gian kết thúc phải sau thời gian bắt đầu ít nhất 30 phút và theo bội số của 30 phút.");
        }
    }

    public class CreateQuestionValidate : AbstractValidator<CreateProductCommand.CreateQuestionCommand>
    {
        public CreateQuestionValidate()
        {
            RuleFor(q => q.Type)
                .NotNull()
                .WithMessage("Loại câu hỏi bắt buộc")
                .IsInEnum()
                .WithMessage("Loại câu hỏi phải là Radio hoặc CheckBox");

            RuleFor(q => q.Description)
                .NotEmpty()
                .WithMessage("Mô tả bắt buộc");

            RuleFor(q => q.Options)
                .NotEmpty()
                .WithMessage("Phải có ít nhất 1 lựa chọn");

            RuleForEach(q => q.Options)
                .SetValidator(new CreateOptionValidate());
        }
    }

    public class CreateOptionValidate : AbstractValidator<CreateProductCommand.CreateOptionCommand>
    {
        public CreateOptionValidate()
        {
            RuleFor(q => q.Description)
                .NotEmpty()
                .WithMessage("Mô tả bắt buộc");

            RuleFor(q => q.IsPricing)
                .NotNull()
                .WithMessage("Lựa chọn có giá hay không");

            RuleFor(q => q.Price)
                .GreaterThan(0).When(q => q.IsPricing)
                .WithMessage("Giá phải lớn hơn 0 nếu lựa chọn có giá")
                .Equal(0).When(q => !q.IsPricing)
                .WithMessage("Giá phải bằng 0 nếu không lựa chọn có giá");
        }
    }
}