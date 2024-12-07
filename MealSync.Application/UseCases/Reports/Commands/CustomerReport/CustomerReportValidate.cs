using FluentValidation;

namespace MealSync.Application.UseCases.Reports.Commands.CustomerReport;

public class CustomerReportValidate : AbstractValidator<CustomerReportCommand>
{
    public CustomerReportValidate()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Id phải lớn hơn 0");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Tiêu đề báo cáo đơn hàng không được để trống")
            .MaximumLength(400)
            .WithMessage("Tiêu đề báo cáo tối đa 400 kí tự");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Nội dung báo cáo đơn hàng không được để trống")
            .MaximumLength(800)
            .WithMessage("Nội dung báo cáo tối đa 800 kí tự.");

        When(x => x.Images != default && x.Images.Any(), () =>
        {
            RuleFor(x => x.Images)
                .Must(images => images.Length <= 5)
                .WithMessage("Tối đa 5 ảnh được phép tải lên.");

            RuleForEach(x => x.Images)
                .Must(file => file.Length <= 5 * 1024 * 1024)
                .WithMessage("Mỗi ảnh phải nhỏ hơn hoặc bằng 5 MB.");
        });
    }
}